#!/bin/bash

DATE=$(date +%F)
BACKUP_DIR="/var/lib/postgresql/backups/"

mkdir -p $BACKUP_DIR

sudo -u postgres pg_dump test_db > $BACKUP_DIR/testdb_$DATE.sql
