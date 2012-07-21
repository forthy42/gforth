/* Android main() for Gforth on Android

  Copyright (C) 2012 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/
#include <android/log.h>
#include <android_native_app_glue.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>

#include "forth.h"

int gforth_server(int portno)
{
     int sockfd, newsockfd;
     socklen_t clilen;
     struct sockaddr_in serv_addr, cli_addr;

     sockfd = socket(AF_INET, SOCK_STREAM, 0);
     if (sockfd < 0) 
       fprintf(stderr, "ERROR opening socket\n");
     bzero((char *) &serv_addr, sizeof(serv_addr));
     portno = atoi(argv[1]);
     serv_addr.sin_family = AF_INET;
     serv_addr.sin_addr.s_addr = INADDR_ANY;
     serv_addr.sin_port = htons(portno);
     if (bind(sockfd, (struct sockaddr *) &serv_addr,
              sizeof(serv_addr)) < 0) 
       fprintf(stderr, "ERROR on binding\n");
     listen(sockfd,5);
     clilen = sizeof(cli_addr);
     newsockfd = accept(sockfd, 
                 (struct sockaddr *) &cli_addr, 
                 &clilen);
     if (newsockfd < 0) 
       fprintf(error, "ERROR on accept\n");

     return newsockfd;
}

#ifdef ANDROID
void android_main(struct android_app* state)
{
  char *argv[] = { "gforth", NULL };
  char *env[] = { "HOME=/sdcard/gforth/home", NULL };
  int sockfd = gforth_socket(4444);

  app_dummy();

  dup2(sockfd, 0); // set socket to stdin
  dup2(sockfd, 1); // set socket to stdout
  dup2(sockfd, 2); // set socket to stderr

  gforth_main(1, argv, env);
}
#else
void main(int argc, char ** argv, char ** env)
{
  int sockfd = gforth_socket(4444);

  dup2(sockfd, 0); // set socket to stdin
  dup2(sockfd, 1); // set socket to stdout
  dup2(sockfd, 2); // set socket to stderr

  gforth_main(argc, argv, env);
}
#endif
