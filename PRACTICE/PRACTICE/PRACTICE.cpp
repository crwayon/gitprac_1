#include <stdio.h>

int main(void) {
    int num = 10;
    int* pNum = &num;

    printf("num 주소 : %p \n", &num);
    printf("pNum안에 있는 값 %p \n", pNum);

    return 0;
}
