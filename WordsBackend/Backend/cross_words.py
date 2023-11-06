import requests
import json
from typing import List, Tuple
import numpy as np
import random
import string

GRID_SIZE = 5
LEN_3 = 2
LEN_4 = 2
LEN_5 = 1
MAX_ATTEMPTS = 1500


def split(s: str, delimiter: str) -> List[str]:
    return s.split(delimiter)


def get_url(num_words: int, length: int) -> str:
    return f"https://random-word-api.vercel.app/api?words={num_words}&length={length}"


def fetch_data(url: str) -> str:
    response = requests.get(url)
    return response.text


def get_words(num_words: int, word_length: int) -> List[str]:
    data = fetch_data(get_url(num_words, word_length))
    words = split(data, ",")
    for i in range(len(words)):
        words[i] = words[i].replace('"', "").replace("[", "").replace("]", "")
    return words


def sort_words_by_length(words: List[str]) -> List[str]:
    words.sort(key=len)
    return words


def can_place_word_horizontal(
    crossword_table: np.ndarray, word: str, row: int, col: int
) -> bool:
    for i in range(len(word)):
        if (
            crossword_table[row, col + i] != "-"
            and crossword_table[row, col + i] != ""
            and crossword_table[row, col + i] != " "
            and crossword_table[row, col + i] != word[i]
        ):
            return False
    return True


def can_place_word_vertical(
    crossword_table: np.ndarray, word: str, row: int, col: int
) -> bool:
    for i in range(len(word)):
        if (
            crossword_table[row + i, col] != "-"
            and crossword_table[row + i, col] != word[i]
        ):
            return False
    return True


def can_place_word_diagonal(
    crossword_table: np.ndarray, word: str, row: int, col: int
) -> bool:
    for i in range(len(word)):
        if (
            crossword_table[row + i, col + i] != "-"
            and crossword_table[row + i, col + i] != word[i]
        ):
            return False
    return True


def place_word_horizontal(
    crossword_table: np.ndarray, word: str, row: int, col: int, index: int
):
    for i in range(len(word)):
        crossword_table[row, col + i] = word[i]


def place_word_vertical(
    crossword_table: np.ndarray, word: str, row: int, col: int, index: int
):
    for i in range(len(word)):
        crossword_table[row + i, col] = word[i]


def place_word_diagonal(
    crossword_table: np.ndarray, word: str, row: int, col: int, index: int
):
    for i in range(len(word)):
        crossword_table[row + i, col + i] = word[i]


def remove_word_horizontal(crossword_table: np.ndarray, word: str, row: int, col: int):
    for i in range(len(word)):
        crossword_table[row, col + i] = "-"


def remove_word_vertical(crossword_table: np.ndarray, word: str, row: int, col: int):
    for i in range(len(word)):
        crossword_table[row + i, col] = "-"


def remove_word_diagonal(crossword_table: np.ndarray, word: str, row: int, col: int):
    for i in range(len(word)):
        crossword_table[row + i, col + i] = "-"


def solve_crossword(crossword_table: np.ndarray, words: List[str], index: int) -> bool:
    if index >= len(words):
        return True

    word = words[index]

    for row in range(GRID_SIZE - len(word) + 1):
        for col in range(GRID_SIZE - len(word) + 1):
            # Snapshot of the board
            original_state = crossword_table.copy()
            
            if can_place_word_horizontal(crossword_table, word, row, col):
                place_word_horizontal(crossword_table, word, row, col, index)
                if solve_crossword(crossword_table, words, index + 1):
                    return True
                # Revert to original state
                crossword_table[:, :] = original_state

            if can_place_word_vertical(crossword_table, word, row, col):
                place_word_vertical(crossword_table, word, row, col, index)
                if solve_crossword(crossword_table, words, index + 1):
                    return True
                # Revert to original state
                crossword_table[:, :] = original_state

            if row == col and can_place_word_diagonal(crossword_table, word, row, col):
                place_word_diagonal(crossword_table, word, row, col, index)
                if solve_crossword(crossword_table, words, index + 1):
                    return True
                # Revert to original state
                crossword_table[:, :] = original_state

    return False



def generate_and_sort_words_5x5() -> List[str]:
    words_length_3 = get_words(LEN_3, 3)
    words_length_4 = get_words(LEN_4, 4)
    words_length_5 = get_words(LEN_5, 5)
    words = words_length_3 + words_length_4 + words_length_5
    words = sort_words_by_length(words)
    return words


def create_crossword_puzzle() -> Tuple[np.ndarray, List[str]]:
    crossword_table = np.full((GRID_SIZE, GRID_SIZE), "-")
    words = []
    for _ in range(MAX_ATTEMPTS):
        words = generate_and_sort_words_5x5()
        if solve_crossword(crossword_table, words, 0):
            return crossword_table, words

    raise RuntimeError(
        "Failed to create crossword puzzle after " + str(MAX_ATTEMPTS) + " attempts."
    )


def gen_crossword_table() -> Tuple[np.ndarray, List[str]]:
    crossword_table, words = create_crossword_puzzle()
    indices = np.where(crossword_table == "-")
    random_chars = [
        random.choice(string.ascii_lowercase) for _ in range(len(indices[0]))
    ]
    crossword_table[indices] = random_chars
    crossword_table = np.apply_along_axis("".join, axis=1, arr=crossword_table)
    return crossword_table.tolist(), words


def random_char():
    return random.choice(string.ascii_lowercase)


