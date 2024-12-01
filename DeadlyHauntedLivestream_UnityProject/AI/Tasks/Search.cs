using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AI{

    public class Search : TaskBase{

        [Header("Search Settings")]
        [SerializeField] private float _searchRadius = 3f;
        [SerializeField] private float _searchTime = 3f;
        [SerializeField] private float _searchSpeed = 5f;
        [SerializeField] private int _searchCycles = 3;

        private const string SEARCH_DONE = "SearchDone";

        private int _searchCounter;

        //SEARCH BEHAVIOR
        public override void Enable(){
            _searchCounter = _searchCycles-1;
            _aiController.ChangeSpeed(_searchSpeed);
            _aiController.MoveToRandomLocation(_searchRadius);
            EventMessenger.StartListening(EM_AIC_ON_TASK_DONE, OnTaskCompleted_Search);
        }

        public override void Disable(){
            _aiController.DefaultSpeed();
            StopAllCoroutines();
            EventMessenger.StopListening(EM_AIC_ON_TASK_DONE, OnTaskCompleted_Search);
        }

        private void OnTaskCompleted_Search(){
            if(_searchCounter >= 1){
                _searchCounter--;
                StartCoroutine(WaitToSearch(_searchTime, _searchRadius));
            }
            else{
                EventMessenger.TriggerEvent(SEARCH_DONE);
                _aiController.AbortTask();
                Disable();
            }
        }

        IEnumerator WaitToSearch(float time, float radius){
            yield return new WaitForSeconds(time);
            _aiController.MoveToRandomLocation(radius);
        }
    }
}