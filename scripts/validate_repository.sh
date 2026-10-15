#!/usr/bin/env bash
set -euo pipefail

project_root="AI Powered NPC Game"
scene="$project_root/Assets/Scenes/MainScene.unity"
build_settings="$project_root/ProjectSettings/EditorBuildSettings.asset"

if rg -n 'gsk_[A-Za-z0-9]+' .; then
  echo "A Groq credential pattern is present in the repository."
  exit 1
fi

if find "$project_root/Assets" -name 'APIConfig.asset' -print -quit | grep -q .; then
  echo "A local APIConfig.asset is committed."
  exit 1
fi

grep -q 'path: Assets/Scenes/MainScene.unity' "$build_settings"
grep -q 'guid: adae37fffc46c418ab9090cce18e5807' "$scene"
grep -q 'currentEmotion: 2' "$scene"

echo "Unity repository validation passed."
