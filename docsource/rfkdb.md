## Overview

The RFKDB store type can be used to manage IBM Key Database Files (KDB) files.  The IBM utility, GSKCAPICMD, is used to read and write certificates from and to the target store and is therefore required to be installed on the server where each KDB certificate store being managed resides, and its location MUST be in the system $Path.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.

## Requirements

TODO Requirements is a required section

