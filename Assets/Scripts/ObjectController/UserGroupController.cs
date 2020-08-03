using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public class UserGroupController : MonoBehaviour
    {
        public enum UserGroupState
        {
            Pause,
            Lecture
        }

        [SerializeField] 
        private GameObject _userPrefab;

        private List<UserController> _users;

        private UserGroupState _groupState = UserGroupState.Pause;

        public IReadOnlyList<UserController> Users => _users;

        public UserGroupState GroupState => _groupState;
        
        public void CreateUsers(RoomThermalManagerBuilder builder)
        {
            _users = new List<UserController>();

            //Create Users
            for (int i = 0; i < OptionsManager.UserCount; ++i)
            {
                GameObject userObject = Instantiate(_userPrefab);
                userObject.transform.parent = transform;
                UserController userController = userObject.GetComponent<UserController>();
                userController.UserGroupController = this;
                builder?.AddThermalObject(userController);

                _users.Add(userController);
            }
        }

        void Update()
        {
            _groupState = !OptionsManager.Lecture ? UserGroupState.Lecture : UserGroupState.Pause;
        }
    }
}
