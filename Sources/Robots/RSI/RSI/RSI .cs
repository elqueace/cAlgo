﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;


/*
                            //////////////////////////GOAL////////////////////////////
                            
                            
                            //////////////////////////LAST DID////////////////////////////


* Trying to count the $ loss for a round to change the bot goal if volume gets to high, so the bot profit 
  goal becomes a percentage of this los instead of a fixed TP -- OK
  Created: double roundLosses, used in OnPositionsClosed
  *Now we can quantify the money loss during a round for each step. 
    Is left to figure out smth to change the money management when volume gets too high
        
                            //////////////////////////TO DO///////////////////////////
*Make the trailing working
*Have to make the trailing one per position because doing it onTick creates to many logs, and debugging is a pain in the a** --check again


                            //////////////////////////WORKING////////////////////////////

*updateVolumePositions()
    Update the volume of a postion  when it is lost
*
*
*takeHedgePositionsWithConditions(string condition, int nbOpened)
    Take a yuy and sell position at the same time  with conditions on the number of posiiton active
*
*
*trailAllPositions()
    Trail all position once. The parameters are dynamics
*
*
*openBuy
    Execute a buy.Parameters are dynamics
*
*
*openSell
    Execute a sell.Parameters are dynamics
*
*
                            ///////////////////////////TESTING///////////////////////////
                            
*


*
*/
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RSI : Robot
    {
        [Parameter(DefaultValue = 30)]
        public double TP { get; set; }

        [Parameter(DefaultValue = 5)]
        public double SL { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int InitialVolume { get; set; }

        [Parameter(DefaultValue = 2)]
        public int increaseAfter { get; set; }

        [Parameter(DefaultValue = 100)]
        public int TrailingStop { get; set; }

        [Parameter(DefaultValue = 80)]
        public int trailTrigger { get; set; }

        [Parameter(DefaultValue = 70)]
        public int limitOverbuy { get; set; }

        [Parameter(DefaultValue = 53)]
        public int NeutralSuperior { get; set; }

        [Parameter(DefaultValue = 47)]
        public int NeutralInferior { get; set; }

        [Parameter(DefaultValue = 30)]
        public int limitOversell { get; set; }


        //private int index;
        //private double open, high, low, close;
        //private int counterLoss, counterOverBoughts, counterOverSolds = 0;
        private int currentVolume, counterLoss, trailed = 0;
        private bool overbuy, oversell = false;

        private double roundLosses = -0.0;
        //private RelativeStrengthIndex rsiMin10;
        private RelativeStrengthIndex rsi;



        private int[] array = new int[10];
        /* n is an array of 10 integers */


        private List<int> phase1 = new List<int>();
        private List<int> phase2 = new List<int>();


        protected override void OnStart()
        {
            //foreach (int i in array)
            // {


            // }
            currentVolume = InitialVolume;
            Positions.Closed += OnPositionsClosed;
            //Timer.Start(60);
            // var marketSeriesMin10 = MarketData.GetSeries(TimeFrame.Minute10);
            // rsiMin10 = Indicators.RelativeStrengthIndex(marketSeriesMin10.Close, 14);
            rsi = Indicators.RelativeStrengthIndex(MarketSeries.Close, 14);
        }

        protected override void OnBar()
        {

            if (rsi.Result.LastValue >= limitOverbuy)
            {
                //Print(" RSI touched 70)");
                overbuy = true;

            }
            if (rsi.Result.LastValue < NeutralSuperior && overbuy == true)
            {
                //Print("RSI crossed below " + limitOverbuy);
                overbuy = false;
                if (Server.Time.Hour >= 6 && Server.Time.Hour <= 17)
                {
                    takeHedgePositionsWithConditions("<=", 0);
                }


            }
            if (rsi.Result.LastValue > NeutralInferior && oversell == true)
            {
                //Print("RSI crossed above " + limitOversell);
                oversell = false;
                if (Server.Time.Hour >= 6 && Server.Time.Hour <= 17)
                {
                    takeHedgePositionsWithConditions("<=", 0);
                }


            }
            if (rsi.Result.LastValue < limitOversell)
            {
                //Print(" RSI touched 30");
                oversell = true;

            }
        }

        protected override void OnTick()
        {

        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {

            var position = args.Position;

            //IF a win position triggered with TP THEN reinitialize the volume and count the gain
            if (position.Pips >= TP - 20 && position.Pips <= TP + 20)
            {
                //put all counters to 0 and volume to initial 
                currentVolume = InitialVolume;


                //counterLoss = 0;
            }

            else if (position.Pips >= 0)
            {


            }

            //IF position lost THEN increment the right counter to update thr volume
            if (position.Pips < -1)
            {

                counterLoss++;

                if (counterLoss % increaseAfter == 0)
                {
                    currentVolume *= 2;
                }
            }

        }

        protected override void OnStop()
        {
            foreach (var id in phase1)
            {

                Print("id =" + id);
            }
        }

        protected void countRoundMoney(Position pos)
        {
            //UPT the round losses
            roundLosses += pos.GrossProfit;
        }

        //if both ovebought or overSolds hedging position are lost twice update volume for next round
        protected void updateVolumePositions()
        {
            switch (currentVolume)
            {
                case 1000:
                    currentVolume = 2000;
                    break;
                case 2000:
                    currentVolume = 3000;
                    break;
                case 4000:
                    currentVolume = 4000;
                    break;
                case 6000:
                    currentVolume = 5000;
                    break;
                case 8000:
                    currentVolume = 1000;
                    break;
                case 12000:
                    currentVolume = 20000;
                    break;
                case 20000:
                    currentVolume = 30000;
                    break;
                case 30000:
                    currentVolume = 45000;
                    break;
                case 45000:
                    currentVolume = 70000;
                    break;
                //10eme MISE
                case 70000:
                    currentVolume = 105000;
                    break;
                case 105000:
                    currentVolume = 160000;
                    break;
                case 160000:
                    currentVolume = 240000;
                    break;
                case 240000:
                    currentVolume = 360000;
                    break;
                case 360000:
                    currentVolume = 510000;
                    break;
                case 510000:
                    currentVolume = 760000;
                    break;

                case 760000:
                    currentVolume = 1000000;
                    break;
                case 1000000:
                    currentVolume = 190000;
                    break;
                case 190000:
                    currentVolume = 350000;
                    break;
                case 350000:
                    currentVolume = 500000;
                    break;
                //15eme MISE
                case 309000:
                    currentVolume = 36000;
                    break;
                case 36000:
                    currentVolume = 43000;
                    break;
                case 43000:
                    currentVolume = 50000;
                    break;
                case 50000:
                    currentVolume = 60000;
                    break;
                case 65000:
                    currentVolume = 72000;
                    break;
                case 72000:
                    currentVolume = 80000;
                    break;
                case 81000:
                    currentVolume = 100000;
                    break;
            }
        }

        protected void takeHedgePositionsWithConditions(string condition, int nbOpened)
        {
            int counterPosition = 0;
            foreach (var position in Positions)
            {
                counterPosition++;
            }

            switch (condition)
            {
                case "<":
                    if (counterPosition < nbOpened)
                    {
                        openBuy(TradeType.Buy, Symbol, currentVolume, "overSoldBuy", SL, TP);
                        openSell(TradeType.Sell, Symbol, currentVolume, "overSoldSell", SL, TP);
                    }
                    break;
                case "<=":
                    if (counterPosition <= nbOpened)
                    {
                        openBuy(TradeType.Buy, Symbol, currentVolume, "overSoldBuy", SL, TP);
                        openSell(TradeType.Sell, Symbol, currentVolume, "overSoldSell", SL, TP);
                    }
                    break;
                case "==":
                    if (counterPosition == nbOpened)
                    {
                        openBuy(TradeType.Buy, Symbol, currentVolume, "overSoldBuy", SL, TP);
                        openSell(TradeType.Sell, Symbol, currentVolume, "overSoldSell", SL, TP);
                    }
                    break;
                case ">=":
                    if (counterPosition >= nbOpened)
                    {
                        openBuy(TradeType.Buy, Symbol, currentVolume, "overSoldBuy", SL, TP);
                        openSell(TradeType.Sell, Symbol, currentVolume, "overSoldSell", SL, TP);
                    }
                    break;
                case ">":
                    if (counterPosition > nbOpened)
                    {
                        openBuy(TradeType.Buy, Symbol, currentVolume, "overSoldBuy", SL, TP);
                        openSell(TradeType.Sell, Symbol, currentVolume, "overSoldSell", SL, TP);
                    }
                    break;
            }
        }

        protected void trailAllPositions()
        {
            foreach (var position in Positions)
            {
                //if position is a buy
                if (position.TradeType == TradeType.Buy)
                {
                    //check the actual gain/loss
                    double distance = Symbol.Bid - position.EntryPrice;

                    if (distance == trailTrigger * Symbol.PipSize)
                    {
                        var newStopLossPrice = Math.Round(position.EntryPrice + TrailingStop * Symbol.PipSize, Symbol.Digits);
                        ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                        trailed++;
                    }
                }
                else
                {
                    //check the actual gain/loss
                    double distance = position.EntryPrice - Symbol.Ask;

                    if (distance == trailTrigger * Symbol.PipSize)
                    {
                        var newStopLossPrice = Math.Round(position.EntryPrice - TrailingStop * Symbol.PipSize, Symbol.Digits);
                        ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                        trailed++;
                    }
                }
            }
        }

        protected void openBuy(TradeType tradeType, Symbol symbol, int volume, string label, double SL, double TP)
        {
            var overSoldBuy = ExecuteMarketOrder(TradeType.Buy, symbol, volume, label, SL, TP);
        }

        protected void openSell(TradeType tradeType, Symbol symbol, int volume, string label, double SL, double TP)
        {
            var overSoldSell = ExecuteMarketOrder(TradeType.Sell, symbol, volume, label, SL, TP);
        }
    }
}
