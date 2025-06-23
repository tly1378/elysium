#!/bin/bash

WORKSPACE=..
LUBAN_DLL=$WORKSPACE/luban/Luban.dll
CONF_PATH=./luban.conf
OUTPUT_CODE_DIR=../tools/src/gen/cfg
OUTPUT_DATA_DIR=../tools/assets/data

dotnet $LUBAN_DLL \
    -t all \
    -d json \
    -c python-json \
    --conf=$CONF_PATH \
    -x outputDataDir=$OUTPUT_DATA_DIR \
    -x outputCodeDir=$OUTPUT_CODE_DIR