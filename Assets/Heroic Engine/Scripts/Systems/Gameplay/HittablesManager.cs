using HeroicEngine.Components.Combat;
using HeroicEngine.Systems.DI;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public sealed class HittablesManager : SystemBase, IHittablesManager
    {
        private readonly Dictionary<TeamType, List<Hittable>> _teamsHittables = new();
        private readonly List<Hittable> _allHittables = new();

        /// <summary>
        /// This method registers hittable object in manager. Other characters will be able to find it.
        /// </summary>
        /// <param name="hittable">Hittable object</param>
        public void RegisterHittable(Hittable hittable)
        {
            if (!_teamsHittables.ContainsKey(hittable.TeamType))
            {
                _teamsHittables.Add(hittable.TeamType, new List<Hittable>
                {
                    hittable
                });
            }
            else if (!_teamsHittables[hittable.TeamType].Contains(hittable))
            {
                _teamsHittables[hittable.TeamType].Add(hittable);
            }

            if (!_allHittables.Contains(hittable))
            {
                _allHittables.Add(hittable);
            }
        }

        /// <summary>
        /// This method unregisters hittable object from manager. Other characters and projectiles will not be able to find it.
        /// </summary>
        /// <param name="hittable">Hittable object</param>
        public void UnregisterHittable(Hittable hittable)
        {
            if (_teamsHittables[hittable.TeamType].Contains(hittable))
            {
                _teamsHittables[hittable.TeamType].Remove(hittable);
            }
            if (_allHittables.Contains(hittable))
            {
                _allHittables.Remove(hittable);
            }
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetHittablesInRadius(Vector3 from, float radius)
        {
            if (_allHittables.Count == 0)
            {
                return new List<Hittable>();
            }

            var selectedHittables = new List<Hittable>();

            foreach (var hittable in _allHittables)
            {
                if (hittable == null || hittable.IsDead())
                {
                    continue;
                }
                if (hittable.transform.position.Distance(from) <= radius)
                {
                    selectedHittables.Add(hittable);
                }
            }

            return selectedHittables;
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point, from all teams except excludedTeam.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <param name="excludedTeam">Excluded team</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetOtherTeamsHittablesInRadius(Vector3 from, float radius, TeamType excludedTeam)
        {
            var result = new List<Hittable>();

            foreach (var teamType in _teamsHittables.Keys)
            {
                if (teamType != excludedTeam)
                {
                    result.AddRange(GetTeamHittablesInRadius(from, radius, teamType));
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point, from certain team.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <param name="teamType">Hittables team</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetTeamHittablesInRadius(Vector3 from, float radius, TeamType teamType)
        {
            if (!_teamsHittables.TryGetValue(teamType, out var teamHittables))
            {
                return new List<Hittable>();
            }

            if (teamHittables.Count == 0)
            {
                return teamHittables;
            }

            var selectedHittables = new List<Hittable>();

            foreach (var hittable in teamHittables)
            {
                if (!hittable || hittable.IsDead())
                {
                    continue;
                }
                if (hittable.transform.position.Distance(from) <= radius)
                {
                    selectedHittables.Add(hittable);
                }
            }

            return selectedHittables;
        }

        /// <summary>
        /// This method instantly kills all Hittable objects of certain team.
        /// </summary>
        /// <param name="teamType">Team</param>
        public void KillTeam(TeamType teamType)
        {
            if (_teamsHittables.TryGetValue(teamType, out var teamsHittable))
            {
                teamsHittable.ForEach(hittable => hittable.Kill());
            }
        }
    }
}
