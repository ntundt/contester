#!/bin/bash

set -xeo pipefail

SWAGGER_CODEGEN_CLI_VERSION=3.0.81

if [ ! -f ./swagger-codegen-cli-$SWAGGER_CODEGEN_CLI_VERSION.jar ]; then
  wget https://repo1.maven.org/maven2/io/swagger/codegen/v3/swagger-codegen-cli/$SWAGGER_CODEGEN_CLI_VERSION/swagger-codegen-cli-$SWAGGER_CODEGEN_CLI_VERSION.jar
fi

wget https://localhost:7115/swagger/v1/swagger.json --no-check-certificate
java -jar ./swagger-codegen-cli-$SWAGGER_CODEGEN_CLI_VERSION.jar generate \
  -i swagger.json \
  -l typescript-angular \
  -o ./ClientApp/src/generated/client/ \
  --additional-properties=ngVersion=21

rm ./swagger.json
