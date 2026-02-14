#!/bin/bash

# ==================================================
# TEST SCRIPT FOR CHAPTER 6: JWT & IDENTITY
# ==================================================

BASE_URL="http://localhost:5273"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Function to print test header
print_header() {
    echo ""
    echo "=========================================="
    echo "$1"
    echo "=========================================="
}

# Function to print test result
print_result() {
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ PASS${NC}: $2"
        PASSED_TESTS=$((PASSED_TESTS + 1))
    else
        echo -e "${RED}✗ FAIL${NC}: $2"
        FAILED_TESTS=$((FAILED_TESTS + 1))
    fi
}

# Function to check if server is running
check_server() {
    print_header "Checking Server Status"
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" $BASE_URL)
    if [ "$HTTP_CODE" != "000" ]; then
        print_result 0 "Server is running at $BASE_URL"
        return 0
    else
        print_result 1 "Server is not responding"
        echo -e "${RED}Please start the server first: cd MvPresentation && dotnet run${NC}"
        exit 1
    fi
}

# ==================================================
# TEST 1: USER REGISTRATION
# ==================================================
test_user_registration() {
    print_header "TEST 1: User Registration"
    
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/user/auth/register" \
        -H "Content-Type: application/json" \
        -d '{
            "userName": "testuser_script",
            "email": "testuser_script@example.com",
            "password": "Test@Password123"
        }')
    
    # Check if response contains access token
    if echo "$RESPONSE" | grep -q "access.*token"; then
        print_result 0 "User registration successful"
        # Extract access token
        USER_ACCESS_TOKEN=$(echo $RESPONSE | grep -oP '(?<="token":")[^"]*' | head -1)
        USER_REFRESH_TOKEN=$(echo $RESPONSE | grep -oP '(?<="token":")[^"]*' | tail -1)
        return 0
    else
        print_result 1 "User registration failed"
        echo "Response: $RESPONSE"
        return 1
    fi
}

# ==================================================
# TEST 2: USER LOGIN
# ==================================================
test_user_login() {
    print_header "TEST 2: User Login"
    
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/user/auth/login" \
        -H "Content-Type: application/json" \
        -d '{
            "email": "testuser_script@example.com",
            "password": "Test@Password123"
        }')
    
    if echo "$RESPONSE" | grep -q "access.*token"; then
        print_result 0 "User login successful"
        USER_ACCESS_TOKEN=$(echo $RESPONSE | grep -oP '(?<="token":")[^"]*' | head -1)
        return 0
    else
        print_result 1 "User login failed"
        return 1
    fi
}

# ==================================================
# TEST 3: GET USER PROFILE (Protected Route)
# ==================================================
test_user_profile() {
    print_header "TEST 3: Get User Profile (Protected Route)"
    
    RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/user/auth/profile" \
        -H "Authorization: Bearer $USER_ACCESS_TOKEN")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "200" ] && echo "$BODY" | grep -q "testuser_script"; then
        print_result 0 "User profile retrieved successfully"
        return 0
    else
        print_result 1 "Failed to get user profile (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 4: ADMIN REGISTRATION
# ==================================================
test_admin_registration() {
    print_header "TEST 4: Admin Registration"
    
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/admin/auth/register" \
        -H "Content-Type: application/json" \
        -d '{
            "userName": "admin_script",
            "email": "admin_script@example.com",
            "password": "Admin@Password123"
        }')
    
    if echo "$RESPONSE" | grep -q "access.*token"; then
        print_result 0 "Admin registration successful"
        ADMIN_ACCESS_TOKEN=$(echo $RESPONSE | grep -oP '(?<="token":")[^"]*' | head -1)
        return 0
    else
        print_result 1 "Admin registration failed"
        return 1
    fi
}

# ==================================================
# TEST 5: ADMIN LOGIN
# ==================================================
test_admin_login() {
    print_header "TEST 5: Admin Login"
    
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/admin/auth/login" \
        -H "Content-Type: application/json" \
        -d '{
            "email": "admin_script@example.com",
            "password": "Admin@Password123"
        }')
    
    if echo "$RESPONSE" | grep -q "access.*token"; then
        print_result 0 "Admin login successful"
        ADMIN_ACCESS_TOKEN=$(echo $RESPONSE | grep -oP '(?<="token":")[^"]*' | head -1)
        return 0
    else
        print_result 1 "Admin login failed"
        return 1
    fi
}

# ==================================================
# TEST 6: ADMIN GET ALL USERS
# ==================================================
test_admin_get_users() {
    print_header "TEST 6: Admin Get All Users"
    
    RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/admin/users" \
        -H "Authorization: Bearer $ADMIN_ACCESS_TOKEN")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "200" ] && echo "$BODY" | grep -q "data"; then
        print_result 0 "Admin retrieved all users successfully"
        USER_COUNT=$(echo "$BODY" | grep -o "userName" | wc -l)
        echo "   → Total users in system: $USER_COUNT"
        return 0
    else
        print_result 1 "Admin failed to get users (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 7: AUTHORIZATION - User Access Admin Endpoint (Should Fail)
# ==================================================
test_authorization_user_to_admin() {
    print_header "TEST 7: Authorization - User tries Admin Endpoint (Should be 403)"
    
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/admin/users" \
        -H "Authorization: Bearer $USER_ACCESS_TOKEN")
    
    if [ "$HTTP_CODE" = "403" ]; then
        print_result 0 "Authorization working - User blocked from Admin endpoint (403 Forbidden)"
        return 0
    else
        print_result 1 "Authorization failed - Expected 403, got $HTTP_CODE"
        return 1
    fi
}

# ==================================================
# TEST 8: INVALID LOGIN (Wrong Password)
# ==================================================
test_invalid_login() {
    print_header "TEST 8: Invalid Login - Wrong Password"
    
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/user/auth/login" \
        -H "Content-Type: application/json" \
        -d '{
            "email": "testuser_script@example.com",
            "password": "WrongPassword123"
        }')
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "400" ] && echo "$BODY" | grep -q "error"; then
        print_result 0 "Invalid login rejected correctly"
        return 0
    else
        print_result 1 "Invalid login not handled properly (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 9: WEAK PASSWORD VALIDATION
# ==================================================
test_weak_password() {
    print_header "TEST 9: Weak Password Validation"
    
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/user/auth/register" \
        -H "Content-Type: application/json" \
        -d '{
            "userName": "weakuser",
            "email": "weakuser@example.com",
            "password": "weak"
        }')
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "400" ]; then
        print_result 0 "Weak password rejected correctly"
        return 0
    else
        print_result 1 "Weak password validation failed (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 10: DUPLICATE EMAIL
# ==================================================
test_duplicate_email() {
    print_header "TEST 10: Duplicate Email Registration"
    
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/user/auth/register" \
        -H "Content-Type: application/json" \
        -d '{
            "userName": "duplicate_user",
            "email": "testuser_script@example.com",
            "password": "Test@Password123"
        }')
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "400" ] && echo "$BODY" | grep -q "error"; then
        print_result 0 "Duplicate email rejected correctly"
        return 0
    else
        print_result 1 "Duplicate email not handled properly (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 11: UNAUTHORIZED ACCESS (No Token)
# ==================================================
test_unauthorized_access() {
    print_header "TEST 11: Unauthorized Access - No Token"
    
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/user/auth/profile")
    
    if [ "$HTTP_CODE" = "401" ]; then
        print_result 0 "Unauthorized access blocked correctly (401)"
        return 0
    else
        print_result 1 "Unauthorized access not handled properly (Expected 401, got $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# TEST 12: REFRESH TOKEN
# ==================================================
test_refresh_token() {
    print_header "TEST 12: Refresh Token"
    
    # First, get a refresh token
    LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/user/auth/login" \
        -H "Content-Type: application/json" \
        -d '{
            "email": "testuser_script@example.com",
            "password": "Test@Password123"
        }')
    
    REFRESH_TOKEN=$(echo $LOGIN_RESPONSE | grep -oP '(?<="token":")[^"]*' | tail -1)
    
    if [ -z "$REFRESH_TOKEN" ]; then
        print_result 1 "Failed to get refresh token"
        return 1
    fi
    
    # Now use refresh token to get new access token
    REFRESH_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/user/auth/refresh" \
        -H "Content-Type: application/json" \
        -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}")
    
    HTTP_CODE=$(echo "$REFRESH_RESPONSE" | tail -n1)
    BODY=$(echo "$REFRESH_RESPONSE" | head -n-1)
    
    if [ "$HTTP_CODE" = "200" ] && echo "$BODY" | grep -q "access.*token"; then
        print_result 0 "Refresh token works correctly"
        return 0
    else
        print_result 1 "Refresh token failed (HTTP $HTTP_CODE)"
        return 1
    fi
}

# ==================================================
# RUN ALL TESTS
# ==================================================

echo ""
echo "╔════════════════════════════════════════════════════════╗"
echo "║   CHAPTER 6: JWT & IDENTITY - AUTOMATED TEST SUITE    ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""

# Check server first
check_server

# Run all tests
test_user_registration
test_user_login
test_user_profile
test_admin_registration
test_admin_login
test_admin_get_users
test_authorization_user_to_admin
test_invalid_login
test_weak_password
test_duplicate_email
test_unauthorized_access
test_refresh_token

# ==================================================
# SUMMARY
# ==================================================

print_header "TEST SUMMARY"
echo ""
echo "Total Tests:  $TOTAL_TESTS"
echo -e "${GREEN}Passed:       $PASSED_TESTS${NC}"
echo -e "${RED}Failed:       $FAILED_TESTS${NC}"
echo ""

if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}╔════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║   ALL TESTS PASSED! ✓                      ║${NC}"
    echo -e "${GREEN}╚════════════════════════════════════════════╝${NC}"
    exit 0
else
    echo -e "${RED}╔════════════════════════════════════════════╗${NC}"
    echo -e "${RED}║   SOME TESTS FAILED! ✗                     ║${NC}"
    echo -e "${RED}╚════════════════════════════════════════════╝${NC}"
    exit 1
fi
