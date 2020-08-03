using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public enum LectureState
    {
        Pause,
        Lecture,
        Nothing
    }

    public interface ILectureStateProvider
    {
        LectureState State { get; }
    }

    public class LsfLectureStateProvider
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

        private readonly LSFInfoSchnittstelle _lsfInterface;
        private DateTime _lastRefresh = DateTime.MinValue;
        private LectureState _lectureState = LectureState.Lecture;
        
        public LsfLectureStateProvider(LSFInfoSchnittstelle lsfInterface)
        {
            _lsfInterface = lsfInterface;
        }



        public LectureState State
        {
            get
            {
                if (DateTime.Now - _lastRefresh > RefreshInterval)
                {
                    try
                    {
                        
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError("", exception);
                    }
                }
            }
        }
    }

    public class UserGroupController : MonoBehaviour
    {
        public enum UserGroupState
        {
            Pause,
            Lecture,
            EndOfLecture
        }

        [SerializeField] 
        private GameObject _userPrefab;

        private List<UserController> _users;

        public IReadOnlyList<UserController> Users => _users;

        public UserGroupState GroupState { get; private set; } = UserGroupState.Pause;

        public void CreateUsers(RoomThermalManagerBuilder builder)
        {
            _users = new List<UserController>();

            if (OptionsManager.UserCount > 1)
            {
                CreateUser(UserController.UserRole.Lecturer);

                for (int i = 1; i < OptionsManager.UserCount; ++i)
                {
                    CreateUser(UserController.UserRole.Student);
                }
            }

            void CreateUser(UserController.UserRole role)
            {
                GameObject userObject = Instantiate(_userPrefab);
                userObject.transform.parent = transform;
                UserController userController = userObject.GetComponent<UserController>();
                userController.Initialize(userGroupController: this, role);
                builder?.AddThermalObject(userController);

                _users.Add(userController);
            }
        }

        void Update()
        {
            GroupState = !OptionsManager.Lecture ? UserGroupState.Lecture : UserGroupState.Pause;
        }
    }
}
