using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlateCounter : BaseCounter{

    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateTaken;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    private float plateSpawnedAmount;
    private float plateSpawnedAmountMax = 4;

    private void Update(){
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax){
            spawnPlateTimer = 0f;

            if (plateSpawnedAmount < plateSpawnedAmountMax){
                plateSpawnedAmount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player){
        if(!player.HasKitchenObject()){
            //player is emptyhanded!
            if(plateSpawnedAmount > 0){
                //there is atleast one plate on the counter
                plateSpawnedAmount--;
                OnPlateTaken?.Invoke(this, EventArgs.Empty);

                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            }
        }
    }

}
