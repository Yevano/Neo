# Neo Bytecode
Neo Bytecode is a universal binary format for representing Neo Chunks. A Chunk is the 'compilation unit' of Neo. A Chunk is typically just a source file but can be loaded from a string as well. This document specifies the entire Neo binary format.

## Header
**4 bytes** - Magic number: ESC, "Neo" or 0x1B4E656F  
**3 bytes** - Version number; first byte is major, second byte is minor, third byte is patch

## Chunk
**string** - name  
**Import List**  
**Procedure** - initializer  
**Variable List**  
**Procedure List**

## Import  
**string** - path  
**string** - alias (may be null)  

## Procedure
**string** - name  
**bool** - exported  
**short** - number of upvalues  
**short** - number of parameters  
**bool** - varargs  
**Line Range List**  
**Instruction List**  
**Constant List**  
**Procedure List**

## Constant
**1 byte** - type of constant  
**constant** - the constant itself

## Variable
**string** - name  
**bool** - exported  

## Line Range
**int** - line  
**int** - start IP  
**int** - end IP  

### Line Range List  
**int** - number of line ranges  
**[Line Range]**  

### Import List  
**int** - number of imports  
**[Import]**  

### Procedure List
**int** - number of procedures  
**[Procedure]**

### Instruction List
**int** - number of instructions  
**[Instruction]**

### Constant List
**int** - number of constants  
**[Constant]**

### Variable List
**int** - number of variables  
**[Variable]**