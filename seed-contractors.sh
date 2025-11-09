#!/bin/bash
# Script to seed 50 contractors via API endpoint
# Usage: ./seed-contractors.sh

API_URL="http://localhost:5292/api/Database/seed-contractors"

echo "🌱 Seeding 50 contractors..."
echo "📡 API URL: $API_URL"
echo ""

response=$(curl -s -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -w "\n%{http_code}")

http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" -eq 200 ]; then
  echo "✅ Success!"
  echo ""
  echo "$body" | jq '.' 2>/dev/null || echo "$body"
  echo ""
  echo "📝 Login credentials:"
  echo "   Email: testcontractor1@testemail.com through testcontractor50@testemail.com"
  echo "   Password: test1234"
else
  echo "❌ Error (HTTP $http_code)"
  echo "$body"
  exit 1
fi
