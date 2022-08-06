BEGIN {FS="\t"}
{
    sub(/FLOATING/,"FLOAT",$4);
    sub(/ /,"-",$4);
    print "answord "$2" "$4" "(($3!="")?substr($3,2,length($3)-2):$2)" \\ "$1
}
