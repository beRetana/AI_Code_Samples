using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Codice.CM.Common;
using UnityEngine;
using UnityEngine.AI;

namespace AI{

    public class Guard : AIActor{

        [SerializeField] private STATE _state;
        [SerializeField] private AnimationClip _searchInRoom;
        [SerializeField] private AnimationClip _lookInRoom;
        [SerializeField] private List<Transform> _stops;

        private List<TaskBase> _behaviorList = new List<TaskBase>();
        private SEQUENCE _sequence = SEQUENCE.startAnim;
        private SEQUENCE _next;
        private GameObject chasingTarget;

        /* 
           These are components the AI will make reference to often. They are used as variables 
           to avoid unnecessary computational work. Unity iterates through the gameobject to get
           the component. Doing this every time the component needs to be called would be computationally expensive.
        */
        private Search _searching;
        private Chase _chasing;
        private HidingSpotsChecker _checkingHidingSpots;
        private AIController _aIController;
        private Animator _animator;

        /* 
           Constants start with the first two letters of their corresponding messenger or category
           if they are not used for messengers, e.g., ANIM_SEARCH for animations only.
        */

        private const string EM_ON_SEARCH_DONE = "SearchDone";
        private const string EM_ON_SIGHTED_OBJECT = "OnDetectedObject";
        private const string EM_ON_HEARD_OBJECT = "SoundDetected";
        private const string EM_ON_LOST_OBJECT = "OnLostObject";
        private const string EM_ON_CHECKING_COMPLETE = "CheckingComplete";
        private const string EM_ON_USING_PHONE = "UsingPhone";
        private const string EM_ON_FAILED_CHASE = "FailedChase";

        private const string OM_TARGET = "Target";
        private const string OM_OBJECT_HEARD = "ObjectHeard";

        private const string DM_SIGNAL_POSITION = "SignalPosition";

        private const string ANIM_LOOK_AROUND = "LookAround";
        private const string ANIM_SEARCH = "Search";

        /* 
           Variables to keep track if the AI has visited a state already during uninturrupted and inturrupted 
           sequencing of behaviors. 
        */

        private bool _playOnce = true;
        private bool _sequenceOnce = true;
        private int _pathIndex = 0;

        private enum STATE{
            Stationary,
            Searching,
            Chasing,
            Sequence,
            CheckHidingSpots
        }

        private enum SEQUENCE{
            startAnim,
            firstPath,
            lookAnim,
            walkBack,
            endAct
        }

        private void Start(){
            _animator = GetComponent<Animator>();
            _aIController = GetComponent<AIController>();
            _searching = GetComponent<Search>();
            _chasing = GetComponent<Chase>();
            _checkingHidingSpots = GetComponent<HidingSpotsChecker>();
            _behaviorList.Append(_searching);
            _behaviorList.Append(_chasing);
            OnEnable();
        }

        private void OnEnable(){
            EventMessenger.StartListening(EM_ON_SIGHTED_OBJECT,OnPlayerDetected);
            EventMessenger.StartListening(EM_ON_LOST_OBJECT, OnPlayerLost);
            EventMessenger.StartListening(EM_ON_USING_PHONE, PhoneDetected);
            EventMessenger.StartListening(EM_ON_HEARD_OBJECT, SoundDetected);
        }

        private void OnDisable(){
            EventMessenger.StopListening(EM_ON_SIGHTED_OBJECT,OnPlayerDetected);
            EventMessenger.StopListening(EM_ON_LOST_OBJECT, OnPlayerLost);
            EventMessenger.StopListening(EM_ON_USING_PHONE, PhoneDetected);
            EventMessenger.StopListening(EM_ON_HEARD_OBJECT, SoundDetected);
        }

        void Update(){

            switch (_state){
                case STATE.Stationary:
                    if (_playOnce){
                        Debug.Log($"AI's STATE: {_state}");
                        _playOnce = false;
                    }
                    break;
                case STATE.Sequence:
                    if (_playOnce){
                        Debug.Log($"AI's STATE: {_state}");
                        switch (_sequence){
                            case SEQUENCE.startAnim:
                                if (_sequenceOnce){
                                    Debug.Log($"AI's SEQUENCE: {_sequence}");
                                    _sequenceOnce = false;
                                    StartCoroutine(PlayAnimation(_lookInRoom, ANIM_LOOK_AROUND, SEQUENCE.firstPath));
                                }
                                break;
                            case SEQUENCE.firstPath:
                                if (_sequenceOnce){
                                    Debug.Log($"AI's SEQUENCE: {_sequence}");
                                    _sequenceOnce = false;
                                    int max = 2;
                                    SEQUENCE nextSequence = SEQUENCE.lookAnim;
                                    _aIController.MoveTo(_stops[_pathIndex].position);
                                    EventMessenger.StartListening(EM_AIC_ON_TASK_DONE, () => GoToLocation(nextSequence, max));
                                }
                                break;
                            case SEQUENCE.lookAnim:
                                if (_sequenceOnce){
                                    Debug.Log($"AI's STATE: {_sequence}");
                                    _sequenceOnce = false;
                                    StartCoroutine(PlayAnimation(_searchInRoom, ANIM_SEARCH, SEQUENCE.walkBack));
                                }
                                break;
                            case SEQUENCE.walkBack:
                                if (_sequenceOnce){
                                    Debug.Log($"AI's STATE: {_sequence}");
                                    _sequenceOnce = false;
                                    int max = 4;
                                    SEQUENCE nextSequence = SEQUENCE.endAct;
                                    _aIController.MoveTo(_stops[_pathIndex].position);
                                    EventMessenger.StartListening(EM_AIC_ON_TASK_DONE, () => GoToLocation(nextSequence, max));
                                }
                                break;
                            case SEQUENCE.endAct:
                                if (_sequenceOnce){
                                    Debug.Log($"AI's STATE: {_sequence}");
                                    _sequenceOnce = false;
                                    gameObject.SetActive(false);
                                }
                                break;
                        }
                        _playOnce = false;
                    }
                    break;
                case STATE.Searching:
                    if (_playOnce){
                        _searching.Enable();
                        EventMessenger.StartListening(EM_ON_SEARCH_DONE, OnSearchDone);
                        Debug.Log($"AI's STATE: {_state}");
                        _playOnce = false;
                    }
                    break;
                case STATE.Chasing:
                    if(_playOnce){
                        chasingTarget = ObjectMessenger.GetGameObject(OM_TARGET);
                        _chasing.SetTarget(chasingTarget);
                        _chasing.Enable();
                        Debug.Log($"AI's STATE: {_state}");
                        _playOnce = false;
                    }
                    break;
                case STATE.CheckHidingSpots:
                    if(_playOnce){
                        _checkingHidingSpots.SetTarget(ObjectMessenger.GetGameObject(OM_TARGET));
                        _checkingHidingSpots.Enable();
                        EventMessenger.StartListening(EM_ON_CHECKING_COMPLETE, TransitionOfBehaviors);
                        Debug.Log($"AI's STATE: {_state}");
                        _playOnce = false;
                    }
                    break;
            }
        }

        IEnumerator PlayAnimation(AnimationClip animationClip, string name, SEQUENCE newState){
            _animator.enabled = true;
            _animator.ResetTrigger(name);
            _animator.SetTrigger(name);
            yield return new WaitForSeconds(animationClip.length);
            _animator.enabled = false;
            _sequence = newState;
            _playOnce = true;
            _sequenceOnce = true;
        }

        private void GoToLocation(SEQUENCE nextSequence, int max)
        {
            if (_pathIndex < max){
                _aIController.MoveTo(_stops[_pathIndex].position);
                _pathIndex++;
            }
            else{
                EventMessenger.StopListening(EM_AIC_ON_TASK_DONE, () => GoToLocation(nextSequence, max));
                _sequence = nextSequence;
                _playOnce = true;
                _sequenceOnce = true;
            }
        }

        private void OnSearchDone(){
            AbortBehaviors();
            _state = STATE.Sequence;
            _playOnce = true;
            _sequenceOnce = true;
        }

        private void OnPlayerLost(){
            _chasing.SetTargetLost();
            EventMessenger.StartListening(EM_ON_FAILED_CHASE,OnChaseFailed);
        }

        private void OnChaseFailed(){
            EventMessenger.StopListening(EM_ON_FAILED_CHASE,OnChaseFailed);
            AbortBehaviors();

            Room playerRoom = Room.CurrentPlayerRoom;
            if (playerRoom != null){
                HidingSpotChecker roomChecker = playerRoom.GetHidingSpotChecker();
                if (roomChecker != null){
                    roomChecker.ActivateRoom();
                }
            }
    
            _state = STATE.CheckHidingSpots;
            _playOnce = true;
        }

        private void OnPlayerDetected(){
            _state = STATE.Chasing;
            _playOnce = true;
            AbortBehaviors();
        }

        private void PhoneDetected(){
            AbortBehaviors();
            _aIController.AbortTask();
            _state = STATE.Chasing;
            _aIController.MoveTo(DataMessenger.GetVector3(DM_SIGNAL_POSITION));
            EventMessenger.StartListening(EM_ON_FAILED_CHASE,OnChaseFailed);
        }

        private void SoundDetected(){
            AbortBehaviors();
            _aIController.AbortTask();
            _state = STATE.Chasing;
            _aIController.MoveTo(ObjectMessenger.GetObjectDetected(OM_OBJECT_HEARD).Instigator.transform.position);
            EventMessenger.StartListening(EM_ON_FAILED_CHASE,OnChaseFailed);
        }

        public override void AbortBehaviors(){
            StopAllCoroutines();
            _animator.enabled = false;
            foreach (TaskBase behavior in _behaviorList){
                behavior.Disable();
            }
        }

        public override void TransitionOfBehaviors(){
            EventMessenger.StopListening(EM_ON_FAILED_CHASE,OnChaseFailed);
            EventMessenger.StopListening(EM_ON_CHECKING_COMPLETE, TransitionOfBehaviors);
            AbortBehaviors();
            _state = STATE.Sequence;
            _playOnce = true;
            _sequenceOnce = true;
        }
    }
}