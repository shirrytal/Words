
#include "main.hpp"
#define GRID_SIZE 5
#define LEN_3 2
#define LEN_4 2
#define LEN_5 1
#define MAX_ATTEMPTS 1000
#include <iostream>
#include <string>
#include <curl/curl.h>
#include <sstream>
#include <vector>
#include <cstring>
using namespace std;



vector<string> split(const string& s, char delimiter) {
    vector<string> tokens;
    string token;
    istringstream tokenStream(s);
    while (getline(tokenStream, token, delimiter)) {
        tokens.push_back(token);
    }
    return tokens;
}


string getUrl(size_t numWords, size_t length) {
    char buffer[100];
    snprintf(buffer, sizeof(buffer), "https://random-word-api.vercel.app/api?words=%zu&length=%zu", numWords, length);
    return string(buffer);
}



size_t writeCallback(void* contents, size_t size, size_t nmemb, string* userp) {
    userp->append((char*)contents, size * nmemb);
    return size * nmemb;
}

string fetchData(const string& url) {
    CURL* curl = curl_easy_init();
    string readBuffer;

    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

        curl_easy_perform(curl);
        curl_easy_cleanup(curl);
    }
    return readBuffer;
}
void removeFromString(string& str, const string& toRemove) {
    size_t pos = string::npos;
    while ((pos = str.find(toRemove)) != string::npos) {
        str.erase(pos, toRemove.length());
    }
}

vector<string> getWords(size_t num_words, size_t word_length) {
    string data = fetchData(getUrl(num_words, word_length));
    vector<string> words = split(data, ',');
    for (size_t i = 0; i < words.size(); i++) {
        removeFromString(words[i], "\"");
        removeFromString(words[i], "[");
        removeFromString(words[i], "]");
    }
    return words;
}



void sortWordsByLength(vector<string>& words) {
    sort(words.begin(), words.end(), [](const string& a, const string& b) {
        return a.length() < b.length();
        });
}

bool canPlaceWordHorizontal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col) {
    for (size_t i = 0; i < word.length(); i++) {
        if (crosswordTable[row][col + i] != '-' && crosswordTable[row][col + i] != word[i]) {
            return false;
        }
    }
    return true;
}

bool canPlaceWordVertical(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col) {
    for (size_t i = 0; i < word.length(); i++) {
        if (crosswordTable[row + i][col] != '-' && crosswordTable[row + i][col] != word[i]) {
            return false;
        }
    }
    return true;
}

bool canPlaceWordDiagonal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col) {
    for (size_t i = 0; i < word.length(); i++) {
        if (crosswordTable[row + i][col + i] != '-' && crosswordTable[row + i][col + i] != word[i]) {
            return false;
        }
    }
    return true;
}

void placeWordHorizontal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        crosswordTable[row][col + i] = word[i];
        charToWord[row][col + i].push_back(index); // add word index to the list of words at this cell
    }
}

void placeWordVertical(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        crosswordTable[row + i][col] = word[i];
        charToWord[row + i][col].push_back(index); // add word index to the list of words at this cell
    }
}

void placeWordDiagonal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        crosswordTable[row + i][col + i] = word[i];
        charToWord[row + i][col + i].push_back(index); // add word index to the list of words at this cell
    }
}

void removeWordHorizontal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        auto& wordsAtCell = charToWord[row][col + i];
        wordsAtCell.erase(remove(wordsAtCell.begin(), wordsAtCell.end(), index), wordsAtCell.end());

        if (wordsAtCell.empty()) {
            crosswordTable[row][col + i] = '-';
        }
    }
}

void removeWordVertical(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        auto& wordsAtCell = charToWord[row + i][col];
        wordsAtCell.erase(remove(wordsAtCell.begin(), wordsAtCell.end(), index), wordsAtCell.end());

        if (wordsAtCell.empty()) {
            crosswordTable[row + i][col] = '-';
        }
    }
}

void removeWordDiagonal(vector<vector<char>>& crosswordTable, const string& word, size_t row, size_t col, vector<vector<vector<int>>>& charToWord, int index) {
    for (size_t i = 0; i < word.length(); i++) {
        auto& wordsAtCell = charToWord[row + i][col + i];
        wordsAtCell.erase(remove(wordsAtCell.begin(), wordsAtCell.end(), index), wordsAtCell.end());

        if (wordsAtCell.empty()) {
            crosswordTable[row + i][col + i] = '-';
        }
    }
}
// Backtracking function to create a crossword puzzle
bool solveCrossword(vector<vector<char>>& crosswordTable, vector<string>& words, size_t index, vector<vector<vector<int>>>& charToWord) {
    if (index >= words.size()) {
        return true;
    }

    string word = words[index];

    for (size_t row = 0; row <= crosswordTable.size() - word.size(); row++) {
        for (size_t col = 0; col <= crosswordTable[row].size() - word.size(); col++) {
            if (canPlaceWordHorizontal(crosswordTable, word, row, col)) {
                placeWordHorizontal(crosswordTable, word, row, col, charToWord, index);
                if (solveCrossword(crosswordTable, words, index + 1, charToWord)) {
                    return true;
                }
                removeWordHorizontal(crosswordTable, word, row, col, charToWord, index);
            }
            if (canPlaceWordVertical(crosswordTable, word, row, col)) {
                placeWordVertical(crosswordTable, word, row, col, charToWord, index);
                if (solveCrossword(crosswordTable, words, index + 1, charToWord)) {
                    return true;
                }
                removeWordVertical(crosswordTable, word, row, col, charToWord, index);
            }
            if (row == col && canPlaceWordDiagonal(crosswordTable, word, row, col)) {
                placeWordDiagonal(crosswordTable, word, row, col, charToWord, index);
                if (solveCrossword(crosswordTable, words, index + 1, charToWord)) {
                    return true;
                }
                removeWordDiagonal(crosswordTable, word, row, col, charToWord, index);
            }
        }
    }
    return false;
}


vector<string> generateAndSortWords5x5() {
    vector<string> words_length_3 = getWords(LEN_3, 3);
    vector<string> words_length_4 = getWords(LEN_4, 4);
    vector<string> words_length_5 = getWords(LEN_5, 5);
    vector<string> words;
    words.insert(words.end(), words_length_3.begin(), words_length_3.end());
    words.insert(words.end(), words_length_4.begin(), words_length_4.end());
    words.insert(words.end(), words_length_5.begin(), words_length_5.end());
    sortWordsByLength(words); // O(nlogn)
    return words;
}


pair<vector<vector<char>>, vector<string>> createCrosswordPuzzle() {
    vector<vector<char>> crosswordTable(GRID_SIZE, vector<char>(GRID_SIZE, '-'));
    vector<string> words;
    for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++) {
        words = generateAndSortWords5x5();
        /**
         * @brief  charToWord is a 3D vector that stores the words that are placed at each cell of the crossword table.
         *        The first two dimensions are the row and column of the cell, and the third dimension is a vector
         *       of integers that represent the indices of the words that are placed at that cell.
         */
        vector<vector<vector<int>>> charToWord(GRID_SIZE, vector<vector<int>>(GRID_SIZE));

        if (solveCrossword(crosswordTable, words, 0, charToWord)) {
            return make_pair(crosswordTable, words);
        }
    }

    throw std::runtime_error("Failed to create crossword puzzle after " + std::to_string(MAX_ATTEMPTS) + " attempts.");
}


void GenCrossWordTable(char** crosswordTableToSend, char** words) {
    pair<vector<vector<char>>, vector<string>> crossword = createCrosswordPuzzle();
    vector<vector<char>> crosswordTable = crossword.first;
    vector<string> wordsVector = crossword.second;
    for (size_t i = 0; i < GRID_SIZE; i++) {
        for (size_t j = 0; j < GRID_SIZE; j++) {
            crosswordTableToSend[i][j] = crosswordTable[i][j];
        }
    }
    for (size_t i = 0; i < wordsVector.size(); i++) {
        strncpy(words[i], wordsVector[i].c_str(), wordsVector[i].length() + 1);
        words[i][wordsVector[i].length()] = '\0'; // remove newline character
    }
}
