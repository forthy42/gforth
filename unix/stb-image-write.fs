\ stb-image bindings

c-library stb-image-write
    s" -O3" add-cflags
    s" m" add-lib
    \c #define STB_IMAGE_WRITE_IMPLEMENTATION
    \c #define STBIW_WINDOWS_UTF8
    \c #include <stb/stb_image_write.h>
    c-function stbi_write_png stbi_write_png s n n n a n -- n
    c-function stbi_write_bmp stbi_write_bmp s n n n a -- n
    c-function stbi_write_tga stbi_write_tga s n n n a -- n
    c-function stbi_write_jpg stbi_write_jpg s n n n a n -- n
    c-function stbi_write_hdr stbi_write_hdr s n n n a -- n

    c-function stbi_flip_vertically_on_write stbi_flip_vertically_on_write n -- void
    \ flag is non-zero to flip data vertically
end-c-library
