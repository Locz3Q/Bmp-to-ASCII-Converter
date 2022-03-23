// pch.cpp: source file corresponding to the pre-compiled header

#include "pch.h"
#include <iostream>

void cppASCIIArt(int lenght, unsigned char* rowsPtrs) {
    int average;
    for (int i = lenght - 1; i >= 0; i--)
    {
        if (i >= 2) {
            average = ((rowsPtrs[i])
                        + (rowsPtrs[i-1])
                        + (rowsPtrs[i-2])) / 3;
            average /= 10;
        }
        else {
            average = rowsPtrs[i] / 10;
        }
        rowsPtrs[i] = average + 32;
    }
}
// When you are using pre-compiled headers, this source file is necessary for compilation to succeed.
