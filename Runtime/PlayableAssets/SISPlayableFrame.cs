﻿using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class SISPlayableFrame : ISerializationCallbackReceiver {

    internal SISPlayableFrame(TimelineClipSISData owner) {
        m_timelineClipSISDataOwner = owner;        
        m_boolProperties = new Dictionary<PlayableFramePropertyID, PlayableFrameBoolProperty>();  
    }

    internal SISPlayableFrame(TimelineClipSISData owner, SISPlayableFrame otherFrame) {
        m_timelineClipSISDataOwner = owner;
        m_boolProperties = otherFrame.m_boolProperties;
        m_localTime = otherFrame.m_localTime;
    }       
    
    
//----------------------------------------------------------------------------------------------------------------------
    #region ISerializationCallbackReceiver
    public void OnBeforeSerialize() {
        if (null != m_boolProperties) {
            m_serializedBoolProperties = new List<PlayableFrameBoolProperty>(m_boolProperties.Count);
            foreach (KeyValuePair<PlayableFramePropertyID, PlayableFrameBoolProperty> kv in m_boolProperties) {
                m_serializedBoolProperties.Add(kv.Value);
            }        
            
        } else {
            m_serializedBoolProperties = new List<PlayableFrameBoolProperty>();            
        }
        
    }

    public void OnAfterDeserialize() {
        m_boolProperties = new Dictionary<PlayableFramePropertyID, PlayableFrameBoolProperty>();
        if (null != m_serializedBoolProperties) {
            foreach (PlayableFrameBoolProperty prop in m_serializedBoolProperties) {
                PlayableFramePropertyID id = prop.GetID();
                m_boolProperties[id] = new PlayableFrameBoolProperty(id, prop.GetValue());
            }            
        } 
        
        if (null == m_marker)
            return;

        m_marker.SetOwner(this);
    }    
    #endregion //ISerializationCallbackReceiver
    

//----------------------------------------------------------------------------------------------------------------------

    internal void Destroy() {
        if (null == m_marker)
            return;

        DeleteMarker();
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetOwner(TimelineClipSISData owner) {  m_timelineClipSISDataOwner = owner;}
    internal TimelineClipSISData GetOwner() {  return m_timelineClipSISDataOwner; }    
    internal double GetLocalTime()                 { return m_localTime; }

    internal int GetIndex() { return m_index; }
    internal void   SetIndexAndLocalTime(int index, double localTime) {
        m_index = index; 
        m_localTime = localTime;        
    }

    internal TimelineClip GetClipOwner() {
        TimelineClip clip = m_timelineClipSISDataOwner?.GetOwner();
        return clip;
    }

//----------------------------------------------------------------------------------------------------------------------
    //Property
    internal bool GetBoolProperty(PlayableFramePropertyID propertyID) {
        if (null!=m_boolProperties && m_boolProperties.ContainsKey(propertyID)) {
            return m_boolProperties[propertyID].GetValue();
        }

        switch (propertyID) {
            case PlayableFramePropertyID.USED: return true;
            case PlayableFramePropertyID.LOCKED: return false;
                default: return false;
        }        
    }
    
    

    internal void SetBoolProperty(PlayableFramePropertyID id, bool val) {
#if UNITY_EDITOR        
        if (GetBoolProperty(id) != val) {
            EditorSceneManager.MarkAllScenesDirty();            
        }
#endif        
        m_boolProperties[id] = new PlayableFrameBoolProperty(id, val);
        
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    internal void Refresh(bool frameMarkerVisibility) {
        TrackAsset trackAsset = m_timelineClipSISDataOwner.GetOwner()?.parentTrack;
        //Delete Marker first if it's not in the correct track (e.g: after the TimelineClip was moved)
        if (null!= m_marker && m_marker.parent != trackAsset) {
            DeleteMarker();
        }

        //Show/Hide the marker
        if (null != m_marker && !frameMarkerVisibility) {
            DeleteMarker();
        } else if (null == m_marker && null!=trackAsset && frameMarkerVisibility) {
            CreateMarker();
        }

        if (m_marker) {
            TimelineClip clipOwner = m_timelineClipSISDataOwner.GetOwner();
            m_marker.Init(this, clipOwner.start + m_localTime);
        }
    }
//----------------------------------------------------------------------------------------------------------------------

    void CreateMarker() {
        TimelineClip clipOwner = m_timelineClipSISDataOwner.GetOwner();
        TrackAsset trackAsset = clipOwner?.parentTrack;
                       
        Assert.IsNotNull(trackAsset);
        Assert.IsNull(m_marker);
               
        m_marker = trackAsset.CreateMarker<FrameMarker>(m_localTime);
    }

    void DeleteMarker() {
        Assert.IsNotNull(m_marker);
        
        //Marker should have parent, but in rare cases, it may return null
        TrackAsset track = m_marker.parent;
        if (null != track) {
            track.DeleteMarker(m_marker);            
        }

        m_marker = null;

    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private List<PlayableFrameBoolProperty> m_serializedBoolProperties;
    [SerializeField] private double m_localTime;    
    [SerializeField] private FrameMarker m_marker = null;     
    [NonSerialized] private TimelineClipSISData m_timelineClipSISDataOwner = null;

    private int m_index;
    
    private Dictionary<PlayableFramePropertyID, PlayableFrameBoolProperty> m_boolProperties;




}

} //end namespace


//A structure to store if we should use the image at a particular frame