#!/bin/bash

BACKUP_DIR="/var/lib/postgresql/backups/"

HOST="localhost"
PORT="5432"
DATABASE="todo_db"
USERNAME="max"
export PGPASSWORD="postgres"

RETENTION_DAYS=7
DATE=$(date +"%Y%m%d_%H%M%S")
BACKUP_PATH="${BACKUP_DIR}/${DATE}"

pg_basebackup -h ${HOST} -p ${PORT} -U ${USERNAME} -D ${BACKUP_PATH} --format=tar --gzip --wal-method=stream --progress --verbose



