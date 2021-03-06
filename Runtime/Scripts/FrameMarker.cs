﻿using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
[CustomStyle("FrameMarker")]
[HideInMenu]
internal class FrameMarker : Marker, INotification {

    internal void Init(SISPlayableFrame controller, double initialTime) {
        m_playableFrameOwner = controller;
        time = initialTime;
    } 
    
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //return false to indicate that this marker has been invalidated
    internal bool Refresh() {
        TimelineClip clip = m_playableFrameOwner?.GetClipOwner();;
        if (clip == null) {
            return false;
        } 
        
        time = clip.start + m_playableFrameOwner.GetLocalTime();
        
        return true;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal void SetOwner(SISPlayableFrame controller) { m_playableFrameOwner = controller; } 
    internal SISPlayableFrame GetOwner() { return m_playableFrameOwner; } 
    
    internal bool IsFrameUsed() {        
        return m_playableFrameOwner.IsUsed();

    }
    internal void SetFrameUsed(bool used) { m_playableFrameOwner.SetUsed(used); }
    
//----------------------------------------------------------------------------------------------------------------------    
    public PropertyName id { get; } //use default implementation

    private SISPlayableFrame m_playableFrameOwner;
       
    //[TODO-sin: 2020-2-7] Refresh the texture immediately when m_playableFrameOwner.useImage is modified
}

} //end namespace


//A visual representation (Marker) of SISPlayableFrame