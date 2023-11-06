CXXFLAGS=-std=c++11 -Wall -Wextra -pedantic -O3 -g
SRC=main.cpp
LDFLAGS=-lcurl
OBJ=$(SRC:.cpp=.o)
LIBRARY_LINUX=CrossWordsPlugin.so
LIBRARY_MAC=CrossWordsPlugin.a
LIBRARY_WIN=CrossWordsPlugin.dll
HEADERS=main.hpp
LDFLAGS+=-L/usr/local/lib
all: $(LIBRARY)
$(LIBRARY): $(OBJ)
	$(CXX) -shared -o $@ $^ $(LDFLAGS)

linux:  $(OBJ)
	$(CXX) -shared -o $(LIBRARY_LINUX) $^ $(LDFLAGS)
mac:  $(OBJ)
	$(CXX) -shared -o $(LIBRARY_MAC) $^ $(LDFLAGS)
win:  $(OBJ)
	$(CXX) -shared -o $(LIBRARY_WIN) $^ $(LDFLAGS)


%.o: %.cpp $(HEADERS)
	$(CXX) $(CXXFLAGS) -fPIC -o $@ -c $<
clean:
	rm -f $(OBJ) $(LIBRARY)