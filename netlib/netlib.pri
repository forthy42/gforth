
net-gethostbyname	c_addr u -- a_addr 	new	ghbn
a_addr=(UCell *) gethostbyname(cstr(c_addr,u,1));

net-gethostbyaddr	c_addr u1 u2 -- a_addr 	new	ghba
a_addr=(UCell *) gethostbyaddr(c_addr,u1,u2);

net-sendto	c_addr1 u1 c_addr2 u2 u5 u3 -- u4	new	net_sendto
u4 = sendto(u3, c_addr1, u1, u5, (struct sockaddr *) c_addr2, u2);

net-send	c_addr1 u1 u5 u3 --u4	new	net_send
u4 = send(u3, c_addr1, u1,u5);

net-recvfrom	c_addr1 u1 c_addr2 u2 u5 u3 -- u4	new	net_recvfrom
UCell len;
len=u2;
u4 = recvfrom(u3, c_addr1, u1, u5, (struct sockaddr *) c_addr2, &len);

net-recv	c_addr1 u1 u5 u3 -- u4	new	net_recv
u4 = recv(u3, c_addr1, u1, u5);

net-connect	c_addr1 u1 u2 -- n1	new	net_connect
n1=connect(u2,(struct sockaddr *) c_addr1,u1);

net-bind	c_addr1 u1 n1 -- n2	new	net_bind
n2=bind(n1,(struct sockaddr *) c_addr1, u1);

net-close	n1 -- n2	new	net_close
n2=close(n1);

net-accept	c_addr1 u1 n2 -- n3	new	net_accept
UCell len;
len=u1;
n3=accept(n2,(struct sockaddr *) c_addr1, &len);

net-listen	n2 n1 -- n3	new	net_listen
n3=listen(n1,n2);

net-socket	u1 u2 u3 -- n1	new	net_socket
n1=socket(u1,u2,u3);

net-setsockopt	u2 u3 c_addr u5 u1 -- n1	mew	net_setsockopt
n1=setsockopt(u1,u2,u3,c_addr,u5);

