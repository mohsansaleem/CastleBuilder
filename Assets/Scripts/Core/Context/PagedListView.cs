﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PG.Core.Context
{
    public abstract class PagedListView<P, M> : MonoBehaviour where P : ListViewItem<M>
    {
        [Header("References")]
        public Button PreviousButton;
        public Button NextButton;

        protected List<M> ItemsInList;
        protected int CurrentPageStart;

        private List<P> _currentSpawnedItems;

        protected virtual int PageSize { get; }

        protected virtual void Start()
        {
            PreviousButton.onClick.AddListener(MoveToPreviousPage);
            NextButton.onClick.AddListener(MoveToNextPage);
        }

        public void Init(List<M> itemsInList)
        {
            ItemsInList = itemsInList;

            if (_currentSpawnedItems == null)
            {
                _currentSpawnedItems = new List<P>();
            }

            // Spawning first page.
            ShiftPage(0);
        }

        private P SpawnAndCache(M model)
        {
            P view = Spawn(model);
            _currentSpawnedItems.Add(view);
            return view;
        }

        private void DeSpawnAndRemoveFromCache(P itemView)
        {
            DeSpawn(itemView);
            _currentSpawnedItems.Remove(itemView);
        }

        protected abstract P Spawn(M model);
        protected abstract void DeSpawn(P itemView);


        protected void MoveToNextPage()
        {
            ShiftPage(CurrentPageStart + PageSize);
        }

        protected void MoveToPreviousPage()
        {
            ShiftPage(CurrentPageStart - PageSize);
        }

        private void ShiftPage(int startIndex)
        {
            if (ItemsInList.Count == 0)
            {
                PreviousButton.interactable = false;
                NextButton.interactable = false;
                return;
            }

            if (startIndex >= ItemsInList.Count)
            {
                startIndex = ItemsInList.Count - 1;
            }
            else if (startIndex < 0)
            {
                startIndex = 0;
            }

            CurrentPageStart = startIndex;
            NextButton.interactable = startIndex + PageSize < ItemsInList.Count;
            PreviousButton.interactable = startIndex > 0;

            int i = 0;
            for (int itemIndex = startIndex;
                itemIndex < ItemsInList.Count && i < startIndex + PageSize;
                i++, itemIndex++)
            {
                // Refresh if we have a view.
                if (i < _currentSpawnedItems.Count)
                {
                    _currentSpawnedItems[i].RefreshData(ItemsInList[itemIndex]);
                }
                else
                {
                    SpawnAndCache(ItemsInList[itemIndex]);
                }
            }


            while (i < _currentSpawnedItems.Count)
            {
                DeSpawnAndRemoveFromCache(_currentSpawnedItems[i]);
            }
        }
    }
}