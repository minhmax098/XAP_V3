using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Model
{
    public int id;
    public string name;
    public string model_file_path;
    public string thumbnail_file_path;
    public bool is_trial_available; 
    // bổ sung thêm 
    // public Model (
    //     int _id, 
    //     string _name, 
    //     string _model_file_path, 
    //     string _thumbnail_file_path, 
    //     bool _is_trial_available
    // ) {
    //     id = _id; 
    //     name = _name; 
    //     model_file_path = _model_file_path; 
    //     thumbnail_file_path = _thumbnail_file_path; 
    //     is_trial_available = _is_trial_available; 
    // }
} 

[System.Serializable]
public class Models
{
    public Model[] models;
}

