using UnityEngine;

public class UserClass : MonoBehaviour
{
}

[System.Serializable]
public class Player
{
    public string id;
    // API 기능 제거: 토큰 필드 주석처리
    public string accessToken;
    public string refreshToken;
    public string playerName;
    public float rating;
    public long ranking;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class GetMyRatingResponse
{
    public string playerId;
    public float rating;
    public long ranking;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class ResponseEntity_GetMyRatingResponse
{
    public string message;
    public GetMyRatingResponse data;
}

[System.Serializable]
public class UserProfileResponse
{
    public string userId;
    public string name;
    // API 기능 제거: 토큰 필드 주석처리
    // public int remainToken;
    public string profile;
    public string role;
}

[System.Serializable]
public class ResponseEntity_UserProfileResponse
{
    public string message;
    public UserProfileResponse data;
}