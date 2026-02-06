#!/bin/bash
# Run all tests for the Cycling Coach skill

echo "🧪 Running Cycling Coach Tests..."
echo ""

dotnet test --nologo --verbosity normal

# Check exit code
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ All tests passed!"
    exit 0
else
    echo ""
    echo "❌ Some tests failed"
    exit 1
fi
