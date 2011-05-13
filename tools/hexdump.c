#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <fcntl.h>

/* hexdump - reads a file and displays it in hex
 * usage: hexdump <file>
 * compile: gcc -Wall -O2 hexdump.c -o hexdump
 * 
 * This is GPLv3 software, do whatever you please with it
 * (c) 2011 spaceape [[ spaceape99@gmail.com ]]
*/

static const int SZREAD = 16;

int    main(int argc, char** argv)
{
   if (argc < 2)
   {
       fprintf(stderr, "Usage: hexdump <file>.\n"
              "Enjoy the ride\n");
   }
 

   int f = open(argv[1], O_RDONLY);
   
   if (f < 0)
   {
       fprintf(stderr, "Failed.\n");
       return -1;
   }

       int    rd;
       int    cnt = 0;
       char*  data = malloc(SZREAD);

       while (rd = read(f, data, SZREAD))
       {
              printf("\033[1;32m%.8x\033[0m ", cnt);

          int x;

          for (x = 0; x != rd; ++x)
               printf("%.2x ", (unsigned char)data[x]);

              printf("\n");

              cnt += rd; 

          if (rd < SZREAD)
              break;
       }

              printf("\033[1;32m%.8x\033[0m \033[1;31mEOL\033[0m\n", cnt);

       free(data);

       return 0;
}
