using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Lesson
{
    // public string id;
    // public string name;
    // public int categoryId;
    // public string description;
    // public int authorId;
    // public int courseId;
    // public int modelId;
    // public string labelFileId;
    // public string thumbnail_file_path;
    // public string introAudioFileId;
    // public string introVideoFileId;
    public int lessonId; 
    public string lessonThumbnail; 
    public string lessonTitle; 
    public int viewed; 
    public int isFavorate; 
    
}

[System.Serializable]
public class Lessons
{
    public Lesson[] categorywithLessons;
}

[System.Serializable]
public class fourTypes
{
    public Lesson[] myFavorates;
    public Lesson[] mostViewed; 
    public Lesson[] recommendedLessons;
    public Lesson[] myLessons; 
}

[System.Serializable]
public class AllLessons
{
    public int code;
    public string message; 
    public fourTypes data; 
}


