using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Enums;
using HeroicEngine.Utils;
using UnityEngine.SceneManagement;

namespace HeroicEngine.Systems.Gameplay
{
    public sealed class CurrenciesManager : SystemBase, ICurrenciesManager
    {
        private const string CURRENCIES_STATE_KEY = "CurrenciesState";
        private const string CURRENCY_CHANGED_EVENT = "CurrencyChanged";

        [SerializeField] private CurrenciesCollection currencies;
        [SerializeField] private bool saveAfterEachChange = true;

        [Inject] private IEventsManager _eventsManager;
        [Inject] private IUIController _uiController;

        private CurrenciesState _currenciesState;

        public bool GetCurrencyInfo(CurrencyType currencyType, out CurrencyInfo currencyInfo)
        {
            currencyInfo = default;

            var idx = currencies.CurrencyInfos.FindIndex(ci => ci.CurrencyType == currencyType.ToString());

            if (idx >= 0)
            {
                currencyInfo = currencies.CurrencyInfos[idx];
                return true;
            }

            return false;
        }

        public void AddCurrency(CurrencyType currencyType, int amount)
        {
            var idx = _currenciesState.Currencies.FindIndex(c => c.CurrencyType == currencyType);

            var newAmount = Mathf.Max(amount, 0);

            if (idx >= 0)
            {
                var currencyState = _currenciesState.Currencies[idx];
                newAmount = Mathf.Max(currencyState.Amount + amount, 0);
                currencyState.Amount = newAmount;
                _currenciesState.Currencies[idx] = currencyState;
            }
            else
            {
                _currenciesState.Currencies.Add(new CurrencyState
                {
                    CurrencyType = currencyType, Amount = newAmount
                });
            }

            _uiController.UpdateCurrencySlot(currencyType, newAmount);
            _eventsManager.TriggerEvent(CURRENCY_CHANGED_EVENT, currencyType, newAmount);

            if (saveAfterEachChange)
            {
                SaveState();
            }
        }

        public void WithdrawCurrency(CurrencyType currencyType, int amount)
        {
            AddCurrency(currencyType, -amount);
        }

        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            var idx = _currenciesState.Currencies.FindIndex(c => c.CurrencyType == currencyType);

            if (idx >= 0)
            {
                return _currenciesState.Currencies[idx].Amount;
            }

            return 0;
        }

        public void SetCurrencyAmount(CurrencyType currencyType, int amount)
        {
            var idx = _currenciesState.Currencies.FindIndex(c => c.CurrencyType == currencyType);

            amount = Mathf.Clamp(amount, 0, amount);

            if (idx >= 0)
            {
                var currencyState = _currenciesState.Currencies[idx];
                currencyState.Amount = amount;
                _currenciesState.Currencies[idx] = currencyState;
            }
            else
            {
                _currenciesState.Currencies.Add(new CurrencyState
                {
                    CurrencyType = currencyType, Amount = amount
                });
            }

            _uiController.UpdateCurrencySlot(currencyType, amount);
            _eventsManager.TriggerEvent(CURRENCY_CHANGED_EVENT, currencyType, amount);

            if (saveAfterEachChange)
            {
                SaveState();
            }
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(CURRENCIES_STATE_KEY, out _currenciesState))
            {
                _currenciesState = new CurrenciesState
                {
                    Currencies = new List<CurrencyState>(currencies.CurrencyInfos.ConvertAll(ci => new CurrencyState
                    {
                        CurrencyType = (CurrencyType)Enum.Parse(typeof(CurrencyType), ci.CurrencyType), Amount = ci.InitialAmount
                    }))
                };
            }
        }

        public void SaveState()
        {
            DataSaver.SavePrefsSecurely(CURRENCIES_STATE_KEY, _currenciesState);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            LoadState();

            foreach (var currencyState in _currenciesState.Currencies)
            {
                _uiController.UpdateCurrencySlot(currencyState.CurrencyType, currencyState.Amount);
                _eventsManager.TriggerEvent(CURRENCY_CHANGED_EVENT, currencyState.CurrencyType, currencyState.Amount);
            }
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    [Serializable]
    public struct CurrenciesState
    {
        public List<CurrencyState> Currencies;
    }

    [Serializable]
    public struct CurrencyState
    {
        public CurrencyType CurrencyType;
        public int Amount;
    }

    [Serializable]
    public struct CurrencyInfo
    {
        [ReadonlyField] public string CurrencyType;
        public Sprite Icon;
        public string Title;
        public int InitialAmount;
    }
}
