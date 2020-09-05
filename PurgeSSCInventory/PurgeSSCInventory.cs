using System;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using TerrariaApi.Server;

namespace PurgeSSCInventory
{
    [ApiVersion(2, 1)]
    public class PurgeSSCInventory : TerrariaPlugin
    {
        public override string Author => "Quinci";

        public override string Description => "Adds a command to let the server console purge the sscinventory table.";

        public override string Name => "PurgeSSCInventory";

        public override Version Version => new Version(1, 0, 0, 0);

        public PurgeSSCInventory(Main game) : base(game)
        {
            Order = 26;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInit);
            }
            base.Dispose(disposing);
        }

        private void OnInit(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("tshock.admin.group", PurgeSSCInventoryTable, "purgessc") { HelpText = "Purges the sscinventory table. Type /confirm or /deny to confirm/deny the deletion." });
        }

        private void PurgeSSCInventoryTable(CommandArgs args)
        {
            if (args.Player != TSPlayer.Server)
            {
                args.Player.SendErrorMessage("You must run this command in the server console.");
                return;
            }
            args.Player.SendInfoMessage("Type /confirm or /deny to confirm/deny the deletion.");
            args.Player.AddResponse("deny", a =>
            {
                args.Player.AwaitingResponse.Remove("confirm");
                args.Player.SendSuccessMessage("You have denied to purge the sscinventory table.");
            });
            args.Player.AddResponse("confirm", a =>
            {
                args.Player.AwaitingResponse.Remove("deny");
                args.Player.SendSuccessMessage("Attempting to delete sscinventory table...");
                try
                {
                    int deletedRows = TShock.CharacterDB.database.Query("DELETE FROM sscinventory");
                    args.Player.SendInfoMessage($"Deleted Rows: {deletedRows}");
                    if (deletedRows > 0)
                    {
                        args.Player.SendSuccessMessage($"Deleted {deletedRows} rows from sscinventory");
                    }
                    else if (deletedRows == 0)
                    {
                        args.Player.SendInfoMessage($"Table sscinventory is already empty!");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Failed to delete from sscinventory: Table doesn't exist or see exception");
                    }
                }
                catch (Exception e)
                {
                    args.Player.SendErrorMessage($"Failed to delete from sscinventory: Table doesn't exist or see exception");
                    TShock.Log.Warn($"PurgeSSCInventory threw an exception:\n{e}");
                }
            });
        }
    }
}