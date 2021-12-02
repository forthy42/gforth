FROM alpine:latest
LABEL maintainer="Bernd Paysan <bernd@net2o.de>"
ENV LANG C.UTF-8

RUN apk add --no-cache pcre libstdc++ && apk add --no-cache --virtual \
    .build-deps build-base autoconf automake git coreutils gcc bison pcre-dev \
    && git clone https://github.com/GeraldWodni/swig.git && cd swig && \
    ./autogen.sh && ./configure --prefix=/usr --exec-prefix=/usr \
    --without-allegrocl --without-chicken --without-clisp --without-cffi \
    --without-csharp --without-d --without-go --without-guile --without-java \
    --without-javascript --without-lua --without-modula3 --without-mzscheme \
    --without-ocaml --without-octave --without-perl --without-php \
    --without-pike --without-python --without-r --without-ruby \
    --without-scilab --without-sexp --without-tcl --without-uffi --without-xml \
    && make && make install && rm -rf swig && apk del .build-deps

CMD [ "swig" ]

VOLUME /workdir
WORKDIR /workdir
