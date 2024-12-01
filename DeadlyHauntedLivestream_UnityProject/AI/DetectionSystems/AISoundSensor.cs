using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

namespace AI{

    public class AISoundSensor : MonoBehaviour{
        [Header("AI Sound Sensor Basic Settings")]
        [SerializeField] private bool _enabledDefaultSoundDetection = true;
        [SerializeField] private bool _enableSoundDetection = true;
        [SerializeField] private float _defaultRangeOfDetection = 10f;

        private const string EM_SOUND_DETECTED = "SoundDetected";
        private const string OM_OBJECT_HEARD = "ObjectHeard";
        
        public void Enable(){
            _enableSoundDetection = true;
        }

        public void Disable(){
            _enableSoundDetection = false;
        }

        public void ReportSound(GameObject instigator, float rangeOfDetection, float strengthOfSound){
            if(_enableSoundDetection){
                EventMessenger.TriggerEvent(EM_SOUND_DETECTED);
                ObjectDetected objectDetected = new ObjectDetected(instigator, rangeOfDetection, strengthOfSound);
                ObjectMessenger.SetObjectDetected(OM_OBJECT_HEARD, objectDetected);
            }
        }

        /* 
           Default method for trigering sound reports but dev might want to modify how 
           and when these events should get called in which case they might want to manually call ReportSound.
        */
        private void OnCollisionEnter(Collision collision){

            if(_enabledDefaultSoundDetection){
                float impulse = collision.impulse.magnitude;
                ReportSound(gameObject, impulse, _defaultRangeOfDetection);
            }
        }
    }
}
