#!/usr/bin/perl -w
# mesh.pl
# Copyright (c) 2011 spaceape
# 
# This software is provided 'as-is', without any express or implied
# warranty. In no event will the authors be held liable for any damages
# arising from the use of this software.
# 
# Permission is granted to anyone to use this software for any purpose,
# including commercial applications, and to alter it and redistribute it
# freely, subject to the following restrictions:
# 
#     1. The origin of this software must not be misrepresented; you must not
#     claim that you wrote the original software. If you use this software
#     in a product, an acknowledgment in the product documentation would be
#     appreciated but is not required.
# 
#     2. Altered source versions must be plainly marked as such, and must not be
#     misrepresented as being the original software.
# 
#     3. This notice may not be removed or altered from any source
#     distribution.
sub showhelp();
sub transfile($);

my $src = $ARGV[0];

if (!defined $src)
{
    showhelp();
    exit(0);
}

my $dst = $ARGV[1];


if (!defined $dst)
{
    $dst = transfile($src);
}

    open(hSRC, $src) or die "Failed to load `$src`";

    open(hDST, ">$dst") or die "Failed to create `$dst`";

    print STDERR "in:\t$src\n";
    print STDERR "out:\t$dst\n";

use constant {
    NOP => 0,
    OP_VTBL => 1, #not used
    OP_TRI => 3,
    OP_QUAD => 4,
    OP_COLOR => 8,


    BIN_MAGIC => 0,
};

    eval {

        #load variables && vertex table
        %hash = 
        (
           verts => [],
           faces => []
        );

        my $ln;
        my($tag, $p, @v);

        for $ln(<hSRC>)
        {
            if ($ln =~ /^#/)
            {
                next;
            }

               ($tag, $p) = $ln =~ /^(\w+)\s+(.+)/;

            if ($tag eq 'v')
            {
                @v = $p =~ /\s*(\S+)/g;
                push (@{ $hash{'verts'} }, [@v]);
            }
            elsif ($tag eq 'f')
            {
                @v = $p =~ /\s*(\S+)/g;
                push (@{ $hash{'faces'} }, [@v]);
            }

        }

                binmode hDST;

                print hDST pack("ccLL", BIN_MAGIC, 0, scalar @{ $hash{'verts'} }, scalar @{ $hash{'faces'} });

        for $v(@{ $hash{'verts'} })
        {
                print hDST pack("fff", @{ $v });
        }

        for $v(@{ $hash{'faces'} })
        {
            if (scalar @{$v} == 3) {print hDST pack("cLLL", OP_TRI, map($_-1, @{ $v }));}
                                   else {print hDST pack("cLLLL", OP_QUAD, map($_-1, @{ $v }));}
        }

                print STDERR "Done.\n";
    };

        close(hDST);

    if ($@)
    {
        unlink $dst;
        print STDERR "Failed.";
    }

    close(hSRC);


sub showhelp()
{
    printf STDERR "Usage: $0 source.obj [dest]\n";
    printf STDERR "Try again.\n";
}

sub transfile($)
{
    my ($b) = $_[0] =~ m/^(.+)(?:\.[^.]*)$/;
    return "$b.mesh";
}