# Morpho Plugin v1.0

Morpho Smart SDK version: 6.14.5.0
Supported ABIs: armeabi-v7a only

## Contents
- morpho-plugin-release.aar  - Original vendor AAR (reference)
- dependencies/classes.jar   - Java classes compiled against Morpho SDK 6.14.5.0
- dependencies/jni/          - Vendor native SDK .so files
- dependencies/*.jar         - MorphoSmart SDK JAR

## Native library
libMorpho.so is built from src/plugins/android/MorphoLib.c and is
binary-identical across all SDK versions. The hash in tng_file_hash.c
covers all versions.

## Build
  MORPHO_SDK_VERSION=1 bash scripts/build_plugin_aars.sh
