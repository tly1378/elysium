#!/bin/bash

# 进入项目目录
cd "$(dirname "$0")/Elysium" || exit 1

# 清理旧构建
dotnet clean

# 发布项目到linux目录
dotnet publish -c Debug -r linux-x64 -o ../Builds/linux --self-contained true

# 复制配置文件到构建目录
cp appsettings.json ../Builds/linux/
cp appsettings.Development.json ../Builds/linux/

echo "打包完成！输出目录: server/Builds/linux"