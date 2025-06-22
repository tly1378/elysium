#!/bin/bash

WORKSPACE=..
LUBAN_DLL=$WORKSPACE/luban/Luban.dll
CONF_PATH=./luban.conf
OUTPUT_CODE_DIR=../client/Assets/Scripts/Gen/Cfg
OUTPUT_DATA_DIR=../client/Assets/StreamingAssets/Configuration

dotnet $LUBAN_DLL \
    -t all \
    -d json \
    -c cs-newtonsoft-json \
    --conf $CONF_PATH \
    -x outputDataDir=$OUTPUT_DATA_DIR \
    -x outputCodeDir=$OUTPUT_CODE_DIR
