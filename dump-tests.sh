# dump-tests.sh  ––– makes a clean “roll-up” of the test sources
# usage:  bash dump-tests.sh   (from repo root)

TEST_DIR="EnviroMonitorApp/EnviroMonitorApp.Tests"

# safety-net: bail if the dir isn’t there
[ -d "$TEST_DIR" ] || { echo "❌  $TEST_DIR not found – run from repo root"; exit 1; }

# collect .cs + .csproj but skip bin/obj noise
find "$TEST_DIR" \
     \( -name '*.cs' -o -name '*.csproj' \) \
     -not -path '*/bin/*' -not -path '*/obj/*' \
| while read -r file; do
    echo -e "\n\n────────────────────────────────────────────────────────────"
    echo "📄  $file"
    echo "────────────────────────────────────────────────────────────"
    cat "$file"
done
