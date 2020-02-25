BIN_DIRECTORY=bin

STDLIB_RESOURCE_FLAGS=$(shell find stdlib/main -type f -exec sh -c "echo {} | sed -e 's/stdlib\/main\///g' -e 's/\.neo//g' -e 's/\//\./g' -e 's~^~/resource:\"{},~g' -e 's/$$/\"/g'" \;)

MAIN_EXECUTABLE=$(BIN_DIRECTORY)/neo.exe
MAIN_MAIN=Neo.CLI.CLI
MAIN_FLAGS=/out:$(MAIN_EXECUTABLE) /main:$(MAIN_MAIN) ${STDLIB_RESOURCE_FLAGS}
MAIN_SOURCES=$(shell find src/main -type f)

TEST_EXECUTABLE=$(BIN_DIRECTORY)/tests.exe
TEST_MAIN=Neo.Tests.Framework.Runner
TEST_FLAGS=/out:$(TEST_EXECUTABLE) /main:$(TEST_MAIN) ${STDLIB_RESOURCE_FLAGS}
TEST_SOURCES+=$(shell find src/main src/test -type f)

STDLIB_TEST_SUITE=stdlib/test/suite.neo

CC=csc

.PHONY: all release debug build-tests test test-stdlib bug-repros clean

all: test test-stdlib bug-repros release

release:
	mkdir -p $(BIN_DIRECTORY)
	$(CC) $(MAIN_SOURCES) $(MAIN_FLAGS) /optimize+

debug:
	mkdir -p $(BIN_DIRECTORY)
	$(CC) $(MAIN_SOURCES) $(MAIN_FLAGS) /debug /optimize-

build-tests:
	mkdir -p $(BIN_DIRECTORY)
	$(CC) $(TEST_SOURCES) $(TEST_FLAGS)

test: build-tests
	mono --debug $(TEST_EXECUTABLE)

test-stdlib: debug
	mono --debug $(MAIN_EXECUTABLE) $(STDLIB_TEST_SUITE)

bug-repros: debug
	mono --debug $(MAIN_EXECUTABLE) bugs/run.neo

clean:
	rm -f $(MAIN_EXECUTABLE)
	rm -f $(TEST_EXECUTABLE)
