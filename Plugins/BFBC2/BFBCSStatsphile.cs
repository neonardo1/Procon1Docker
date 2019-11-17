/*  Copyright 2010 Geoffrey 'Phogue' Green

    This file is part of BFBC2 PRoCon.

    BFBC2 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BFBC2 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BFBC2 PRoCon.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.ComponentModel;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class BFBCSStatsphile : PRoConPluginAPI, IPRoConPluginInterface {

        private bool m_isPluginEnabled;

        private Dictionary<string, PlayerInformation> m_dicPlayers;

        private enumBoolYesNo m_enAllowGeneralPublicToShoutDetails;
        // STATS Timeout-recheck?

        private Hashtable m_defaultHashTable = null;

        public BFBCSStatsphile() {
            this.m_enAllowGeneralPublicToShoutDetails = enumBoolYesNo.Yes;

			this.m_defaultHashTable = null;

            this.m_dicPlayers = new Dictionary<string, PlayerInformation>();
        }

        public string GetPluginName() {
            return "BFBCS Statsphile In-Game API";
        }

        public string GetPluginVersion() {
            return "1.1.0.0";
        }

        public string GetPluginAuthor() {
            return "Phogue";
        }

        public string GetPluginWebsite() {
            return "www.phogue.net";
        }

        public string GetPluginDescription() {
            return @"<p>For support regarding this plugin please visit <a href=""http://www.phogue.net"" target=""_blank"">www.phogue.net</a></p>

<p><a href=""http://bfbcs.com"" target=""_blank""><img src=""http://files2.bfbcs.com/img/bfbcs/banner_468.jpg"" width=""468"" height=""60"" border=""0"" alt=""BFBC2 Stats"" /></a></p>

<h2>Description</h2>
<p>Provides an in game interface to the <a href=""http://www.bfbcs.com"" target=""_blank"">bfbcs.com</a> API so players on your server can pull global stats about other players in the server.</p>

<h2>Requirements</h2>
        <h4>Plugin sandbox mode (Tools -> Options -> Plugins -> Plugin Security)</h4>
            <ul>
                <li>URL: http://api.bfbcs.com</li>
                <li>Port: 80</li>
            </ul>
    <p>If you have your procon hosted you will need to nag your service provider.</p>

<h2>Command Response Scopes</h2>
    <blockquote><h4>!</h4>Shouts the response back to all players.. the ""gloating"" response.  Thankfully you can limit it to people with accounts only if you want by disabling the ""General Public"" option =)</blockquote>
    <blockquote><h4>@</h4>Privately responds to the speaker with the requested statistics.</blockquote>

    <p>All error messages are privately sent to the speaker</p>

<h2>Commands</h2>
    <blockquote><h4>@bfbcs</h4>Gives a summary of the speaking player</blockquote>
    <blockquote><h4>@bfbcs [playername]</h4>Gives a summary of another player</blockquote>
    <blockquote><h4>@bfbcs [playername] [stat]</h4>Information about a weapon/vehicle/etc for a given player</blockquote>
    <blockquote><h4>@bfbcs [player1] [vs] [player2]</h4>Compares player1 to player2</blockquote>
    <blockquote><h4>@bfbcs [player1] [vs] [player2] [stat]</h4>Compares player1 to player2 on a stat</blockquote>

<h2>Settings</h2>
    <h3>Miscellaneous</h3>
        <blockquote><h4>Allow general public to shout responses from the server</h4>Enables/Disables the ! response scope.  If you disable public players being able to shout responses the command will revert to whispering the results instead.  Account holders will still be able to shout responses using !</blockquote>

<h2><a href=""http://www.bfbcs.com"" target=""_blank"">bfbcs.com is</a></h2>

<h4>Programming</h4>
    <ul>
        <li>Dominik ""1ApRiL"" Herbst</li>
    </ul>

<h4>Graphics</h4>
    <ul>
        <li>&lt;-BS-&gt;</li>
    </ul>

<h4>Optimization</h4>
    <ul>
        <li>Michael Renner</li>
    </ul>

<h4>Stats field analysis</h4>
    <ul>
        <li>Dominik ""1ApRiL"" Herbst</li>
        <li>Markus ""Vestalis"" Gubitz</li>
        <li>Tobias ""Kantholz"" Düthorn</li>
        <li>bf-games.net</li>
    </ul>
<h4>Testing & Debugging</h4>
    <ul>
        <li>Juri ""ContraViZe"" Dinges</li>
        <li>Stephan ""TeZwo"" Urbanski</li>
    </ul>

<h2>Additional Information</h2>
    <ul>
        <li>This plugin is compatible with Basic In-Game Info's @help command</li>
        <li>The [stat] can be favourites or a gadget, weapon, vehicle, insignia, kit, specialization or a pin</li>
        <li>The [playername] must be in the game.  This plugin is simply to provide quick information, you're much better off going to <a href=""http://www.bfbcs.com"" target=""_blank"">bfbcs.com</a> if you want to research some one =)</li>
        <li>Procon will only nag bfbcs once and will store the stats until the player leaves.  To prevent unnecessary spam to bfbcs.com only if requested will a player be fetched so there might be a 5-10 second delay when checking a new player for the first time</li>
    </ul>
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {

        }

        public void OnPluginEnable() {

#region Show us ya text strings
			if (this.m_defaultHashTable == null) {
				// Pulled from a string to easily update if bfbcs.com is updated.
				this.m_defaultHashTable = PlayerInformation.AdditionalPrepOnHashtable((Hashtable)JSON.JsonDecode(@"{""players"":[{""name"":""Phogue"",""platform"":""pc"",""rank"":29,""rank_name"":""SECOND LIEUTENANT II"",""veteran"":0,""score"":1048862,""level"":154,""kills"":4952,""deaths"":2567,""time"":331162.362,""elo"":437.333,""form"":0,""date_lastupdate"":""2010-06-18T23:10:10+00:00"",""date_lastcheck"":""2010-06-18T23:10:10+00:00"",""lastcheck"":""updated"",""count_updates"":12,""general"":{""accuracy"":1.716,""dogr"":72,""dogt"":135,""elo0"":399.436,""elo1"":402.653,""games"":271,""goldedition"":0,""losses"":120,""sc_assault"":68541,""sc_award"":641870,""sc_bonus"":41582,""sc_demo"":26384,""sc_general"":292720,""sc_objective"":24840,""sc_recon"":133541,""sc_squad"":48220,""sc_support"":144406,""sc_team"":11940,""sc_vehicle"":34120,""slevel"":0,""spm"":0,""spm0"":0,""spm1"":0,""srank"":0,""sveteran"":0,""teamkills"":17,""udogt"":0,""wins"":151},""kits"":{""assault"":{""name"":""Assault"",""score"":68541,""kills"":917,""deaths"":468},""demo"":{""name"":""Engineer"",""score"":26384,""kills"":340,""deaths"":225},""support"":{""name"":""Medic"",""score"":144406,""kills"":1573,""deaths"":664},""recon"":{""name"":""Recon"",""score"":133541,""kills"":1982,""deaths"":1056}},""teams"":{""att"":{""name"":""Attacker"",""loss"":16,""win"":125},""def"":{""name"":""Defender"",""loss"":103,""win"":22}},""weapons"":{""aek"":{""name"":""AEK-971 Vintovka"",""kills"":106,""shots_fired"":7732,""shots_hit"":1432,""seconds"":97633.122,""headshots"":29,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""xm8"":{""name"":""XM8 Prototype"",""kills"":165,""shots_fired"":7434,""shots_hit"":1711,""seconds"":11470.799,""headshots"":45,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""f2000"":{""name"":""F2000 Assault"",""kills"":1,""shots_fired"":74,""shots_hit"":22,""seconds"":20.9,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""aug"":{""name"":""Stg.77 AUG"",""kills"":222,""shots_fired"":9595,""shots_hit"":1842,""seconds"":23246.228,""headshots"":60,""stars"":{""bron"":1,""silv"":1,""gold"":2,""plat"":0}},""an94"":{""name"":""AN-94 Abakan"",""kills"":117,""shots_fired"":3170,""shots_hit"":678,""seconds"":5607.492,""headshots"":26,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""m416"":{""name"":""M416"",""kills"":14,""shots_fired"":441,""shots_hit"":154,""seconds"":483.634,""headshots"":2,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m16"":{""name"":""M16A2"",""kills"":1,""shots_fired"":132,""shots_hit"":18,""seconds"":469.853,""headshots"":1,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""9a91"":{""name"":""9A-91 Avtomat"",""kills"":35,""shots_fired"":1116,""shots_hit"":214,""seconds"":2843.567,""headshots"":5,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""scar"":{""name"":""SCAR-L Carbine"",""kills"":60,""shots_fired"":2146,""shots_hit"":330,""seconds"":3229.266,""headshots"":10,""stars"":{""bron"":1,""silv"":1,""gold"":0,""plat"":0}},""xm8c"":{""name"":""XM8 Compact"",""kills"":1,""shots_fired"":0,""shots_hit"":0,""seconds"":101.033,""headshots"":1,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""aks74u"":{""name"":""AKS-74U Krinkov"",""kills"":48,""shots_fired"":2309,""shots_hit"":480,""seconds"":3166.636,""headshots"":12,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""uzi"":{""name"":""UZI"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""pp2"":{""name"":""PP-2000 Avtomat"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""ump"":{""name"":""UMP-45"",""kills"":1,""shots_fired"":50,""shots_hit"":10,""seconds"":44.233,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""pkm"":{""name"":""PKM LMG"",""kills"":102,""shots_fired"":4134,""shots_hit"":796,""seconds"":5538.966,""headshots"":20,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""m249"":{""name"":""M249 Saw"",""kills"":579,""shots_fired"":36182,""shots_hit"":7262,""seconds"":14219.832,""headshots"":122,""stars"":{""bron"":1,""silv"":1,""gold"":5,""plat"":0}},""qju88"":{""name"":""Type 88 LMG"",""kills"":80,""shots_fired"":4206,""shots_hit"":832,""seconds"":2029.633,""headshots"":18,""stars"":{""bron"":1,""silv"":1,""gold"":0,""plat"":0}},""m60"":{""name"":""M60 LMG"",""kills"":495,""shots_fired"":18923,""shots_hit"":3312,""seconds"":17854.435,""headshots"":111,""stars"":{""bron"":1,""silv"":1,""gold"":4,""plat"":0}},""xm8lmg"":{""name"":""XM8 LMG"",""kills"":4,""shots_fired"":150,""shots_hit"":42,""seconds"":84.133,""headshots"":1,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mg36"":{""name"":""MG36"",""kills"":2,""shots_fired"":166,""shots_hit"":22,""seconds"":48.833,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mg3"":{""name"":""MG3"",""kills"":165,""shots_fired"":10561,""shots_hit"":1712,""seconds"":7362.034,""headshots"":36,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""m24"":{""name"":""M24 Sniper"",""kills"":334,""shots_fired"":1986,""shots_hit"":910,""seconds"":15591.599,""headshots"":141,""stars"":{""bron"":1,""silv"":1,""gold"":3,""plat"":0}},""qbu88"":{""name"":""Type 88 Sniper"",""kills"":1,""shots_fired"":16,""shots_hit"":4,""seconds"":22.667,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""sv98"":{""name"":""SV98 Snaiperskaya"",""kills"":3,""shots_fired"":20,""shots_hit"":12,""seconds"":124.833,""headshots"":1,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""svu"":{""name"":""SVU Snaiperskaya Short"",""kills"":1,""shots_fired"":8,""shots_hit"":0,""seconds"":457.733,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""gol"":{""name"":""GOL Sniper Magnum"",""kills"":990,""shots_fired"":3675,""shots_hit"":1633,""seconds"":60824.969,""headshots"":317,""stars"":{""bron"":1,""silv"":1,""gold"":9,""plat"":1}},""vss"":{""name"":""VSS Snaiperskaya Special"",""kills"":21,""shots_fired"":798,""shots_hit"":152,""seconds"":900.9,""headshots"":7,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m95"":{""name"":""M95 Sniper"",""kills"":1,""shots_fired"":0,""shots_hit"":0,""seconds"":1157.777,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m9"":{""name"":""M9 Pistol"",""kills"":4,""shots_fired"":73,""shots_hit"":31,""seconds"":1865.799,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mcs"":{""name"":""870 Combat"",""kills"":79,""shots_fired"":502,""shots_hit"":200,""seconds"":5261.599,""headshots"":5,""stars"":{""bron"":1,""silv"":1,""gold"":0,""plat"":0}},""s12k"":{""name"":""Saiga 20k Semi"",""kills"":19,""shots_fired"":1630,""shots_hit"":272,""seconds"":1996.499,""headshots"":1,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mp443"":{""name"":""MP-443 Grach"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m1911"":{""name"":""WWII M1911 .45"",""kills"":442,""shots_fired"":5709,""shots_hit"":1698,""seconds"":9115.179,""headshots"":68,""stars"":{""bron"":1,""silv"":1,""gold"":8,""plat"":0}},""m1a1"":{""name"":""WWII M1A1 Thompson"",""kills"":0,""shots_fired"":37,""shots_hit"":8,""seconds"":304.367,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mp412"":{""name"":""MP-412 Rex"",""kills"":2,""shots_fired"":30,""shots_hit"":8,""seconds"":27.9,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m93r"":{""name"":""M93R Burst"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":13.667,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""spas12"":{""name"":""SPAS-12 Combat"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mk14ebr"":{""name"":""M14"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""g3"":{""name"":""G3"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""u12"":{""name"":""USAS-12 Auto"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m1"":{""name"":""M1"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""n2k"":{""name"":""NEOSTEAD 2000 Combat"",""kills"":4,""shots_fired"":42,""shots_hit"":8,""seconds"":162.533,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m16k"":{""name"":""M162A - SPECTACT"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""mg3k"":{""name"":""MG3 - SPECTACT"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m95k"":{""name"":""M95 - SPECTACT"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""umpk"":{""name"":""UMP - SPECTACT"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""seconds"":0,""headshots"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}}},""vehicles"":{""hmv"":{""name"":""HMMWV 4WD"",""class"":""light"",""kills"":0,""roadkills"":0,""seconds"":197.701,""distance"":1081.141,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""vodn"":{""name"":""VODNIK 4WD"",""class"":""light"",""kills"":0,""roadkills"":0,""seconds"":22.9,""distance"":230.131,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""cobr"":{""name"":""COBRA 4WD"",""class"":""light"",""kills"":0,""roadkills"":0,""seconds"":0,""distance"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""quad"":{""name"":""Quad Bike"",""class"":""light"",""kills"":0,""roadkills"":0,""seconds"":4739.769,""distance"":19924.965,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m1a2"":{""name"":""M1A2 Abrams"",""class"":""heavy"",""kills"":51,""roadkills"":1,""seconds"":5975.066,""distance"":20289.888,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""t90"":{""name"":""T-90 MBT"",""class"":""heavy"",""kills"":52,""roadkills"":1,""seconds"":5239.5,""distance"":14539.946,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""m3a3"":{""name"":""M3A3 Bradley"",""class"":""heavy"",""kills"":90,""roadkills"":2,""seconds"":7959.812,""distance"":12906.593,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""bmd3"":{""name"":""BMD-3 Bakhcha"",""class"":""heavy"",""kills"":12,""roadkills"":0,""seconds"":1015.568,""distance"":2836.776,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""bmda"":{""name"":""BMD-3 Bakhcha AA"",""class"":""heavy"",""kills"":2,""roadkills"":0,""seconds"":10.933,""distance"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""jets"":{""name"":""Personal Water Craft"",""class"":""water"",""kills"":0,""roadkills"":0,""seconds"":261.501,""distance"":2429.363,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""PBLB"":{""name"":""Patrol Boat"",""class"":""water"",""kills"":1,""roadkills"":0,""seconds"":272.7,""distance"":1251.884,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""ah60"":{""name"":""UH-60 Transport"",""class"":""air"",""kills"":14,""roadkills"":0,""seconds"":3227.069,""distance"":23004.453,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""ah64"":{""name"":""AH-64 Apache"",""class"":""air"",""kills"":2,""roadkills"":0,""seconds"":2313.634,""distance"":14013.117,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""MI28"":{""name"":""MI-28 Havoc"",""class"":""air"",""kills"":4,""roadkills"":0,""seconds"":991.9,""distance"":9058.477,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""havoc"":{""name"":""Mi-24 Hind"",""class"":""air"",""kills"":1,""roadkills"":0,""seconds"":488.267,""distance"":3434.311,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""uav"":{""name"":""UAV"",""class"":""air"",""kills"":9,""roadkills"":0,""seconds"":2531.335,""distance"":10498.765,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""XM312"":{""name"":""Heavy MG X312"",""class"":""stationary"",""kills"":2,""seconds"":41.266,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""KORD"":{""name"":""Heavy MG KORD"",""class"":""stationary"",""kills"":12,""seconds"":919.067,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""KORN"":{""name"":""Stationary AT KORN"",""class"":""stationary"",""kills"":6,""seconds"":356.803,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""TOW2"":{""name"":""Stationary AT TOW2"",""class"":""stationary"",""kills"":0,""seconds"":133.767,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""aav"":{""name"":""Anti-Air Gun"",""class"":""stationary"",""kills"":1,""seconds"":851.333,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}}},""vehicle_class"":{""light"":{""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""heavy"":{""stars"":{""bron"":1,""silv"":1,""gold"":4,""plat"":0}},""water"":{""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""air"":{""stars"":{""bron"":1,""silv"":1,""gold"":0,""plat"":0}},""stationary"":{""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}}},""gadgets"":{""40mmgl"":{""name"":""40MM Grenade"",""unlocked"":1,""kit"":""assault"",""kills"":59,""shots_fired"":706,""shots_hit"":219,""headshots"":0,""seconds"":13596.69,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""ammb"":{""name"":""Ammo Box"",""unlocked"":1,""kit"":""assault"",""resupplies"":195,""stars"":[]},""smol"":{""name"":""40MM Smoke Launcher"",""unlocked"":1,""kit"":""assault"",""kills"":0,""shots_fired"":435,""shots_hit"":0,""headshots"":0,""seconds"":1114.206,""stars"":[]},""40mmsg"":{""name"":""40MM Shotgun"",""unlocked"":1,""kit"":""assault"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""headshots"":0,""seconds"":0,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""rpg7"":{""name"":""RPG-7 AT"",""unlocked"":1,""kit"":""demo"",""kills"":15,""shots_fired"":115,""shots_hit"":75,""headshots"":0,""seconds"":958.334,""stars"":{""bron"":1,""silv"":0,""gold"":0,""plat"":0}},""rept"":{""name"":""Repair Tool"",""unlocked"":1,""kit"":""demo"",""repairs"":34,""kills"":27,""shots_fired"":4072,""shots_hit"":1826,""headshots"":0,""seconds"":1910.933,""stars"":[]},""atm"":{""name"":""Anti-Tank Mine"",""unlocked"":1,""kit"":""demo"",""kills"":0,""shots_fired"":6,""shots_hit"":0,""headshots"":0,""seconds"":14.567,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""m2cg"":{""name"":""M2 Carl Gustav AT"",""unlocked"":1,""kit"":""demo"",""kills"":70,""shots_fired"":312,""shots_hit"":280,""headshots"":0,""seconds"":2570.066,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""m136"":{""name"":""M136 AT4"",""unlocked"":1,""kit"":""demo"",""kills"":2,""shots_fired"":18,""shots_hit"":8,""headshots"":0,""seconds"":526.633,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""medk"":{""name"":""Medic kit"",""unlocked"":1,""kit"":""medic"",""heals"":988,""stars"":[]},""defi"":{""name"":""Defibrillator"",""unlocked"":1,""kit"":""medic"",""revives"":329,""kills"":19,""shots_fired"":700,""shots_hit"":34,""headshots"":5,""seconds"":1564.235,""stars"":[]},""mots"":{""name"":""Motion Sensor"",""unlocked"":1,""kit"":""recon"",""kills"":0,""shots_fired"":721,""shots_hit"":2534,""headshots"":0,""seconds"":1496.494,""stars"":[]},""c4"":{""name"":""C4 Explosive"",""unlocked"":1,""kit"":""recon"",""kills"":110,""shots_fired"":906,""shots_hit"":552,""headshots"":0,""seconds"":3151.547,""stars"":{""bron"":1,""silv"":1,""gold"":2,""plat"":0}},""mst"":{""name"":""Mortar Strike"",""unlocked"":1,""kit"":""recon"",""kills"":0,""shots_fired"":192,""shots_hit"":16,""headshots"":0,""seconds"":92.666,""stars"":{""bron"":0,""silv"":0,""gold"":0,""plat"":0}},""knv"":{""name"":""Combat Knife"",""unlocked"":1,""kit"":""all"",""kills"":134,""shots_fired"":937,""shots_hit"":217,""headshots"":0,""seconds"":965.629,""stars"":{""bron"":1,""silv"":1,""gold"":2,""plat"":0}},""hgr"":{""name"":""Hand Grenade"",""unlocked"":1,""kit"":""all"",""kills"":80,""shots_fired"":1316,""shots_hit"":418,""headshots"":0,""seconds"":1514.365,""stars"":{""bron"":1,""silv"":1,""gold"":1,""plat"":0}},""trad"":{""name"":""Tracer Dart Gun"",""unlocked"":1,""kit"":""all"",""kills"":0,""shots_fired"":19,""shots_hit"":0,""headshots"":0,""seconds"":90.365,""stars"":[]},""bay"":{""name"":""M1 Bayonette"",""unlocked"":false,""kit"":""all"",""kills"":0,""shots_fired"":0,""shots_hit"":0,""headshots"":0,""seconds"":0,""stars"":[]}},""specializations"":{""assault_r"":{""name"":""Red Dot Sight"",""unlocked"":1,""kit"":""assault"",""criteria1"":{""label"":""Assault Score"",""value"":68541,""target"":14000}},""assault_s"":{""name"":""4x Rifle Scope"",""unlocked"":1,""kit"":""assault"",""criteria1"":{""label"":""Assault Score"",""value"":68541,""target"":17000}},""assault_a"":{""name"":""Marksman Assault Rifle Training"",""unlocked"":1,""kit"":""assault"",""criteria1"":{""label"":""Assault Score"",""value"":68541,""target"":24000}},""smg_r"":{""name"":""Red Dot Sight"",""unlocked"":1,""kit"":""demo"",""criteria1"":{""label"":""Engineer Score"",""value"":26384,""target"":11100}},""smg_s"":{""name"":""4x Rifle Scope"",""unlocked"":1,""kit"":""demo"",""criteria1"":{""label"":""Engineer Score"",""value"":26384,""target"":13500}},""smg_a"":{""name"":""Marksman SMG Training"",""unlocked"":1,""kit"":""demo"",""criteria1"":{""label"":""Engineer Score"",""value"":26384,""target"":21500}},""medheal"":{""name"":""Medkit Heal+"",""unlocked"":1,""kit"":""medic"",""criteria1"":{""label"":""Medic Score"",""value"":144406,""target"":12000}},""lmg_r"":{""name"":""Red Dot Sight"",""unlocked"":1,""kit"":""medic"",""criteria1"":{""label"":""Medic Score"",""value"":144406,""target"":15000}},""lmg_s"":{""name"":""4x Rifle Scope"",""unlocked"":1,""kit"":""medic"",""criteria1"":{""label"":""Medic Score"",""value"":144406,""target"":18000}},""medradius"":{""name"":""Medkit Radius+"",""unlocked"":1,""kit"":""medic"",""criteria1"":{""label"":""Medic Score"",""value"":144406,""target"":21000}},""lmg_aim"":{""name"":""Marksman LMG Training"",""unlocked"":1,""kit"":""medic"",""criteria1"":{""label"":""Medic Score"",""value"":144406,""target"":28000}},""sniper_s"":{""name"":""4x Rifle Scope"",""unlocked"":1,""kit"":""recon"",""criteria1"":{""label"":""Recon Score"",""value"":133541,""target"":8000}},""sczmplus"":{""name"":""12x High Power Scope"",""unlocked"":1,""kit"":""recon"",""criteria1"":{""label"":""Recon Score"",""value"":133541,""target"":10000}},""sniper_r"":{""name"":""Red Dot Sight"",""unlocked"":1,""kit"":""recon"",""criteria1"":{""label"":""Recon Score"",""value"":133541,""target"":12500}},""spscope"":{""name"":""Sniper Spotter"",""unlocked"":1,""kit"":""recon"",""criteria1"":{""label"":""Recon Score"",""value"":133541,""target"":17000}},""sprint"":{""name"":""Lightweight Combat Edquipment"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":5}},""ammsupp"":{""name"":""Ammo Hip Bandolier"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":6}},""grsupp"":{""name"":""Grenade Vest"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":7}},""bodarm"":{""name"":""Ceramic Body Armor"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":14}},""buldmplus"":{""name"":""Magnum Ammunition"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":15}},""expdmplus"":{""name"":""Improved Demolitions"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":17}},""expsupp"":{""name"":""Explosives Leg Pouch"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":8}},""shotgun_s"":{""name"":""12 Gauge Slugs"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":16}},""shotgun_c"":{""name"":""Extended Shotgun Magazine"",""unlocked"":1,""kit"":""all"",""criteria1"":{""label"":""Rank"",""value"":29,""target"":9}},""vehmosens"":{""name"":""Electronic Warfare Package"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":1200}},""harveharm"":{""name"":""Active Armor Upgrade"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":2300}},""vdamage"":{""name"":""Improved Warheads Package"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":3600}},""vreload"":{""name"":""Quick Reload Package"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":5000}},""tnsmk"":{""name"":""Smoke Countermeasures"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":6500}},""tnzm"":{""name"":""High Power Optics Package"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":8000}},""coaxmg"":{""name"":""Alternate Weapon Package"",""unlocked"":1,""kit"":""vehicle"",""criteria1"":{""label"":""Vehicle Score"",""value"":34120,""target"":10000}}},""insiginias"":[{""name"":""Elite Marksman Combat"",""count"":1,""criteria1"":{""label"":""# of headshots"",""value"":500,""target"":500}},{""name"":""Distinguished Sidearm Combat"",""count"":1,""criteria1"":{""label"":""# of kills with pistols"",""value"":100,""target"":100}},{""name"":""Distinguished Grenade Combat"",""count"":0,""criteria1"":{""label"":""# of kills with grenades"",""value"":82,""target"":100}},{""name"":""Distinguished Melee Combat"",""count"":1,""criteria1"":{""label"":""# of knife kills"",""value"":100,""target"":100}},{""name"":""Elite Melee Combat"",""count"":0,""criteria1"":{""label"":""# of kills with knives"",""value"":135,""target"":200}},{""name"":""Distinguished Anti-Vehicle Combat"",""count"":0,""criteria1"":{""label"":""# of vehicles destroyed"",""value"":155,""target"":500}},{""name"":""Long Service Assault Weapons Combat"",""count"":1,""criteria1"":{""label"":""# of assault rifles kills"",""value"":500,""target"":500}},{""name"":""Long Service Support Weapons Combat"",""count"":1,""criteria1"":{""label"":""# of LMG kills"",""value"":500,""target"":500}},{""name"":""Anti Vehicle Combat"",""count"":0,""criteria1"":{""label"":""# of vehicle damages"",""value"":155,""target"":200}},{""name"":""Long Service Radio Warfare"",""count"":0,""criteria1"":{""label"":""# of tracer plants"",""value"":6,""target"":100}},{""name"":""Distinguished Retaliation Combat"",""count"":1,""criteria1"":{""label"":""# of payback pins"",""value"":5,""target"":5},""criteria2"":{""label"":""# of nemesis pins"",""value"":6,""target"":5}},{""name"":""Exemplary Marksman Combat"",""count"":0,""criteria1"":{""label"":""# of marksman headshots"",""value"":16,""target"":100}},{""name"":""Distinguished Remote Explosives Combat"",""count"":1,""criteria1"":{""label"":""# of kills with C4"",""value"":100,""target"":100}},{""name"":""Long Service Sniping Weapons Combat"",""count"":1,""criteria1"":{""label"":""# of sniper rifle kills"",""value"":500,""target"":500}},{""name"":""Long Service Tactical Weapons Combat"",""count"":0,""criteria1"":{""label"":""# of shotgun kills"",""value"":102,""target"":500}},{""name"":""Long Service Medical Ops"",""count"":1,""criteria1"":{""label"":""# of Heals"",""value"":100,""target"":100}},{""name"":""Long Service Resupply Ops"",""count"":1,""criteria1"":{""label"":""# of Resupplies"",""value"":100,""target"":100}},{""name"":""Long Service Surveilance Ops"",""count"":0,""criteria1"":{""label"":""# of Motion Mine Spot Assists"",""value"":79,""target"":100}},{""name"":""Long Service Maintenance Ops"",""count"":0,""criteria1"":{""label"":""# of Repairs"",""value"":34,""target"":100}},{""name"":""Combat Veteran"",""count"":0,""criteria1"":{""label"":""# of kills"",""value"":4952,""target"":5000}},{""name"":""Distinguished Combat Efficiency"",""count"":1},{""name"":""Distinguished Combat Excellence"",""count"":1},{""name"":""Long Service Light Weapons Combat"",""count"":0,""criteria1"":{""label"":""# of SMG kills"",""value"":145,""target"":500}},{""name"":""Distinguished Explosives Combat"",""count"":0,""criteria1"":{""label"":""# of kills with mines"",""value"":0,""target"":100}},{""name"":""Elite Multiple Target Combat"",""count"":1,""criteria1"":{""label"":""# of double kills"",""value"":50,""target"":50}},{""name"":""Distinguished Marksman Combat"",""count"":1,""criteria1"":{""label"":""# of headshots"",""value"":100,""target"":100}},{""name"":""Superior Service Duty"",""count"":1,""time"":{""label"":""Time Online"",""value"":86400.953,""target"":86400}},{""name"":""Distinguished Service Duty"",""count"":1,""time"":{""label"":""Time Online"",""value"":172800.859,""target"":172800}},{""name"":""Elite Service Duty"",""count"":0,""time"":{""label"":""Time Online"",""value"":332470.156,""target"":432000}},{""name"":""Conquest Good Conduct"",""count"":0,""criteria1"":{""label"":""# of Flags Captured"",""value"":96,""target"":100}},{""name"":""Rush Good Conduct"",""count"":1,""criteria1"":{""label"":""# of M-COMs destroyed"",""value"":100,""target"":100}},{""name"":""Exemplary Weapon Service"",""count"":0},{""name"":""Exemplary Combat Service"",""count"":0},{""name"":""Exemplary Vehicle Service"",""count"":0},{""name"":""Elite Service"",""count"":0},{""name"":""Distinguished Weapon Knowledge"",""count"":0},{""name"":""Distinguished Vehicle Knowledge"",""count"":0},{""name"":""Distinguished Artillery Combat"",""count"":0,""criteria1"":{""label"":""# of kills using mortar strike"",""value"":0,""target"":100}},{""name"":""Distinguished Battlefield Knowledge"",""count"":0},{""name"":""Exemplary Battlefield Knowledge"",""count"":1},{""name"":""Elite Battlefield Knowledge"",""count"":1},{""name"":""Valorous Battlefield Knowledge"",""count"":0},{""name"":""Squad Combat Assistance"",""count"":1,""criteria1"":{""label"":""# of Squad Assists"",""value"":100,""target"":100}},{""name"":""Squad Resupply Ops"",""count"":1,""criteria1"":{""label"":""# of Squad Resupplies"",""value"":100,""target"":100}},{""name"":""Squad Repair Ops"",""count"":0,""criteria1"":{""label"":""# of Squad Repairs"",""value"":23,""target"":100}},{""name"":""Squad Medical Ops"",""count"":1,""criteria1"":{""label"":""# of Squad Heals"",""value"":129,""target"":50},""criteria2"":{""label"":""# of Squad Revives"",""value"":50,""target"":50}},{""name"":""Squad Surveillence Ops"",""count"":1,""criteria1"":{""label"":""# of Squad Motion Mine Spot Assists"",""value"":50,""target"":50}},{""name"":""Squad Tactical Ops"",""count"":1,""criteria1"":{""label"":""# of Squad Spawns"",""value"":200,""target"":200}},{""name"":""Squad Retaliation Ops"",""count"":1,""criteria1"":{""label"":""# of Squad Avenges"",""value"":20,""target"":20},""criteria2"":{""label"":""# of Squad Assists"",""value"":51,""target"":50}},{""name"":""Squad Teamwork Ops"",""count"":0,""criteria1"":{""label"":""# of Squad Attack Orders"",""value"":21,""target"":20},""criteria2"":{""label"":""# of Squad Defend Orders"",""value"":16,""target"":20}},{""name"":""Assault SPECACT Knowledge"",""count"":0,""criteria1"":{""label"":""# of kills"",""value"":0,""target"":100}},{""name"":""Engineer SPECACT Knowledge"",""count"":0,""criteria1"":{""label"":""# of kills"",""value"":0,""target"":100}},{""name"":""Medic SPECACT Knowledge"",""count"":0,""criteria1"":{""label"":""# of kills"",""value"":0,""target"":100}},{""name"":""Recon SPECACT Knowledge"",""count"":0,""criteria1"":{""label"":""# of kills"",""value"":0,""target"":100}}],""pins"":[{""name"":""Assault Rifle Efficiency Pin"",""count"":57},{""name"":""Grenade Launcher Efficiency Pin"",""count"":6},{""name"":""Sniper Rifle Efficiency Pin"",""count"":136},{""name"":""Handgun Efficiency Pin"",""count"":58},{""name"":""Shotgun Efficiency Pin"",""count"":18},{""name"":""Rocket Launcher Efficiency Pin"",""count"":11},{""name"":""Light Machine Gun Efficiency Pin"",""count"":161},{""name"":""Submachine Gun Efficiency Pin"",""count"":11},{""name"":""Emplacement Efficiency Pin"",""count"":0},{""name"":""Explosive Efficiency Pin"",""count"":15},{""name"":""Melee Efficiency Pin"",""count"":1},{""name"":""Anti Vehicle Efficiency Pin"",""count"":4},{""name"":""Combat Efficiency Pin"",""count"":210},{""name"":""Combat Excellence Pin"",""count"":97},{""name"":""Kill Assist Pin"",""count"":42},{""name"":""Savior Pin"",""count"":69},{""name"":""Avenger Pin"",""count"":46},{""name"":""Marksman Pin"",""count"":102},{""name"":""Ace Pin"",""count"":83},{""name"":""Gold Squad Pin"",""count"":128},{""name"":""Nemesis Pin"",""count"":91},{""name"":""Payback! Pin"",""count"":5},{""name"":""Squad Member Pin"",""count"":0},{""name"":""Wheels of Hazard Pin"",""count"":0},{""name"":""Car Warfare Pin"",""count"":0},{""name"":""Tank Warfare Pin"",""count"":8},{""name"":""Naval Warfare Pin"",""count"":0},{""name"":""Air Warfare Pin"",""count"":2},{""name"":""M-Com Attacker Pin"",""count"":13},{""name"":""M-Com Defender Pin"",""count"":57},{""name"":""Rush Winner Pin"",""count"":51},{""name"":""Conquest Winner Pin"",""count"":11},{""name"":""Squad Deathmatch Winner Pin"",""count"":89},{""name"":""Squad Rush Winner Pin"",""count"":0},{""name"":""Flag Attacker Pin"",""count"":18},{""name"":""Flag Defender Pin"",""count"":0},{""name"":""Resupply Ops Pin"",""count"":10},{""name"":""Surveillance Ops Pin"",""count"":5},{""name"":""Medical Ops Pin"",""count"":29},{""name"":""Maintenance Ops Pin"",""count"":3},{""name"":""SPECACT Assault Excellence Pin"",""count"":0},{""name"":""SPECACT Engineer Excellence Pin"",""count"":0},{""name"":""SPECACT Medic Excellence Pin"",""count"":0},{""name"":""SPECACT Recont Excellence Pin"",""count"":0},{""name"":""M16 SPECACT Efficiency Pin"",""count"":0},{""name"":""UMP SPECACT Efficiency Pin"",""count"":0},{""name"":""MG3 SPECACT Efficiency Pin"",""count"":0},{""name"":""M95 SPECACT Efficiency Pin"",""count"":0}],""achievements"":[{""name"":""I Knew We'd Make It"",""unlocked"":1},{""name"":""Retirement just got postponed."",""unlocked"":1},{""name"":""It's bad for my karma, man!"",""unlocked"":1},{""name"":""They got all your intel?"",""unlocked"":1},{""name"":""Salvage a vehicle."",""unlocked"":1},{""name"":""Alright, here it is."",""unlocked"":1},{""name"":""Nobody ever drowned in sweat."",""unlocked"":1},{""name"":""Ghost rider's here!"",""unlocked"":1},{""name"":""Sierra Foxtrot 1079."",""unlocked"":1},{""name"":""Thanks for the smokes, brother!"",""unlocked"":1},{""name"":""Save me some cheerleaders."",""unlocked"":1},{""name"":""Turn on a light."",""unlocked"":1},{""name"":""P.S. Invasion cancelled, sir."",""unlocked"":1},{""name"":""It sucks to be right."",""unlocked"":0},{""name"":""New Shiny Gun"",""unlocked"":1},{""name"":""Guns Guns Guns"",""unlocked"":1},{""name"":""Link to the Past"",""unlocked"":1},{""name"":""Communication Issues"",""unlocked"":0},{""name"":""Complete Blackout"",""unlocked"":0},{""name"":""Ten Blades"",""unlocked"":0},{""name"":""Taxi!"",""unlocked"":1},{""name"":""Destruction"",""unlocked"":1},{""name"":""Destruction Part 2"",""unlocked"":1},{""name"":""Demolish"",""unlocked"":1},{""name"":""Demolish Part 2"",""unlocked"":0},{""name"":""Assault Rifle Aggression"",""unlocked"":1},{""name"":""Sub Machine Gun Storm"",""unlocked"":0},{""name"":""Light Machine Gun Lash Out"",""unlocked"":0},{""name"":""Sniper Rifle Strike"",""unlocked"":1},{""name"":""Wall of Shotgun"",""unlocked"":0},{""name"":""Multiplayer Knowledge"",""unlocked"":1,""criteria1"":{""label"":""# of ranks"",""value"":10,""target"":10}},{""name"":""Multiplayer Elite"",""unlocked"":1,""criteria1"":{""label"":""# of ranks"",""value"":22,""target"":22}},{""name"":""Assault Expert"",""unlocked"":1,""criteria1"":{""label"":""# of weapons"",""value"":8,""target"":3}},{""name"":""Engineer Expert"",""unlocked"":1,""criteria1"":{""label"":""# of weapons"",""value"":8,""target"":3}},{""name"":""Medic Expert"",""unlocked"":1,""criteria1"":{""label"":""# of weapons"",""value"":9,""target"":3}},{""name"":""Recon Expert"",""unlocked"":1,""criteria1"":{""label"":""# of weapons"",""value"":8,""target"":3}},{""name"":""Battlefield Expert"",""unlocked"":1},{""name"":""15 minutes of Fame"",""unlocked"":1,""criteria1"":{""label"":""# of minutes online"",""value"":906.666,""target"":900}},{""name"":""Mission... Accomplished."",""unlocked"":0},{""name"":""Pistol Man"",""unlocked"":0,""criteria1"":{""label"":""# of M9 kills"",""value"":4,""target"":5},""criteria2"":{""label"":""# of M1911 kills"",""value"":5,""target"":5},""criteria3"":{""label"":""# of MP443 kills"",""value"":0,""target"":5},""criteria4"":{""label"":""# of MP412 kills"",""value"":2,""target"":5},""criteria5"":{""label"":""# of M93r kills"",""value"":0,""target"":5}},{""name"":""Airkill"",""unlocked"":0,""criteria1"":{""label"":""# of helicopter roadkills"",""value"":0,""target"":1}},{""name"":""Battlefield Expert"",""unlocked"":1,""criteria1"":{""label"":""# of friend list kills"",""value"":5,""target"":5}},{""name"":""Demolition Man"",""unlocked"":0,""criteria1"":{""label"":""# of demolish kills"",""value"":6,""target"":20}},{""name"":""Careful Guidence"",""unlocked"":0,""criteria1"":{""label"":""# of RPG v. Heli kills"",""value"":0,""target"":1}},{""name"":""The Dentist"",""unlocked"":0,""criteria1"":{""label"":""# of repair tool headshot kills"",""value"":0,""target"":1}},{""name"":""Won Them All"",""unlocked"":1},{""name"":""Squad Player"",""unlocked"":1},{""name"":""Combat Service Support"",""unlocked"":1,""criteria1"":{""label"":""# of resupplies"",""value"":18,""target"":10},""criteria2"":{""label"":""# of heals"",""value"":12,""target"":10},""criteria3"":{""label"":""# of revives"",""value"":34,""target"":10},""criteria4"":{""label"":""# of repairs"",""value"":10,""target"":10},""criteria5"":{""label"":""# of motion mine spot assists"",""value"":10,""target"":10}},{""name"":""Award Aware"",""unlocked"":1,""criteria1"":{""label"":""# of unique awards"",""value"":10,""target"":10}},{""name"":""Award Addicted"",""unlocked"":1,""criteria1"":{""label"":""# of unique awards"",""value"":50,""target"":50}},{""name"":""SPECACT Assault Elite"",""unlocked"":0},{""name"":""SPECACT Engineer Elite"",""unlocked"":0},{""name"":""SPECACT Medic Elite"",""unlocked"":0},{""name"":""SPECACT Recon Elite"",""unlocked"":0}],""queue"":false}],""requested"":1,""found"":1}"), "Phogue");
			}
#endregion
		
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bBFBCS Statsphile In-Game API ^2Enabled!");

            this.m_isPluginEnabled = true;
            this.RegisterAllCommands();
        }

        public void OnPluginDisable() {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bBFBCS Statsphile In-Game API ^1Disabled =(");

            this.m_isPluginEnabled = false;
            this.UnregisterAllCommands();
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables() {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Allow general public to shout responses from the server", typeof(enumBoolYesNo), this.m_enAllowGeneralPublicToShoutDetails));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables() {
            return GetDisplayPluginVariables();
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) {
            if (strVariable.CompareTo("Allow general public to shout responses from the server") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true) {
                this.m_enAllowGeneralPublicToShoutDetails = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

            this.RegisterAllCommands();
        }

        private void UnregisterAllCommands() {
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>()));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>()))));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>()), new MatchArgumentFormat("stat", new List<string>()))));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("player1", new List<string>()), new MatchArgumentFormat("vs", new List<string>()), new MatchArgumentFormat("player2", new List<string>()), new MatchArgumentFormat("stat", new List<string>()))));
            this.UnregisterCommand(new MatchCommand(this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("player1", new List<string>()), new MatchArgumentFormat("vs", new List<string>()), new MatchArgumentFormat("player2", new List<string>()))));
        }

        private void RegisterAllCommands() {

            if (this.m_isPluginEnabled == true) {

                List<string> lstTypes = new List<string>();

                lstTypes.Add("Favourites");
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "gadgets"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "weapons"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "vehicles"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "insiginias"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "kits"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "specializations"));
                lstTypes.AddRange(PlayerInformation.CompileNamesWithinCategory(this.m_defaultHashTable, "pins"));

                this.RegisterCommand(new MatchCommand("BFBCSStatsphile", "OnCommandBfbcs", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Summary of your stats"));
                this.RegisterCommand(new MatchCommand("BFBCSStatsphile", "OnCommandBfbcsOnPlayer", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(this.m_dicPlayers.Keys))), new ExecutionRequirements(ExecutionScope.All), "Summary a players stats"));
                this.RegisterCommand(new MatchCommand("BFBCSStatsphile", "OnCommandBfbcsOnPlayerOnType", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(this.m_dicPlayers.Keys)), new MatchArgumentFormat("stat", lstTypes)), new ExecutionRequirements(ExecutionScope.All), "Stats on a specific favourites/weapon/kit/etc a player"));

                this.RegisterCommand(new MatchCommand("BFBCSStatsphile", "OnCommandBfbcsOnComparePlayers", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("player1", new List<string>(this.m_dicPlayers.Keys)), new MatchArgumentFormat("vs", this.Listify<string>("vs", "versus")), new MatchArgumentFormat("player2", new List<string>(this.m_dicPlayers.Keys))), new ExecutionRequirements(ExecutionScope.All), "Compares one player to another"));
                this.RegisterCommand(new MatchCommand("BFBCSStatsphile", "OnCommandBfbcsOnComparePlayersOnStat", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("player1", new List<string>(this.m_dicPlayers.Keys)), new MatchArgumentFormat("vs", this.Listify<string>("vs", "versus")), new MatchArgumentFormat("player2", new List<string>(this.m_dicPlayers.Keys)), new MatchArgumentFormat("stat", lstTypes)), new ExecutionRequirements(ExecutionScope.All), "Compares one players stats to another"));

                /*
                if (this.m_enAllowGeneralPublicToShoutDetails == enumBoolYesNo.Yes) {
                    this.RegisterCommand(new MatchCommand("BFBCSInGame", "OnCommandBfbcs", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Gives a basic summary of your stats.  ! = Shout, @ = Private."));
                }
                else {
                    this.RegisterCommand(new MatchCommand("BFBCSInGame", "OnCommandBfbcs", this.Listify<string>("@", "!", "#"), "bfbcs", this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Gives a basic summary of your stats."));
                }
                */

            }
        }


        // Player events
        public void OnPlayerJoin(string strSoldierName) {

            if (this.m_dicPlayers.ContainsKey(strSoldierName) == false) {
                this.m_dicPlayers.Add(strSoldierName, new PlayerInformation(new CPlayerInfo(strSoldierName, "", 0, 24), null, null));
            }

            this.RegisterAllCommands();
        }


        public void OnPlayerLeft(string strSoldierName) {

            if (this.m_dicPlayers.ContainsKey(strSoldierName) == true) {
                this.m_dicPlayers.Remove(strSoldierName);
            }

            this.RegisterAllCommands();
        }


        public void OnPunkbusterPlayerInfo(CPunkbusterInfo cpbiPlayer) {

            if (cpbiPlayer != null) {

                if (this.m_dicPlayers.ContainsKey(cpbiPlayer.SoldierName) == false) {
                    this.m_dicPlayers.Add(cpbiPlayer.SoldierName, new PlayerInformation(null, cpbiPlayer, null));
                }
                else {
                    this.m_dicPlayers[cpbiPlayer.SoldierName].PunkbusterInfo = cpbiPlayer;
                }

                this.RegisterAllCommands();
            }
        }


        public void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset) {

            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All) {
                foreach (CPlayerInfo cpiPlayer in lstPlayers) {
                    if (this.m_dicPlayers.ContainsKey(cpiPlayer.SoldierName) == true) {
                        this.m_dicPlayers[cpiPlayer.SoldierName].VanillaInfo = cpiPlayer;
                    }
                    else {
                        this.m_dicPlayers.Add(cpiPlayer.SoldierName, new PlayerInformation(cpiPlayer, null, null));
                    }
                }

                this.RegisterAllCommands();
            }
        }

        #region IPRoConPluginInterface2

        //
        // IPRoConPluginInterface2
        //
        public void OnPlayerKilled(Kill kKillerVictimDetails) {

        }

        public void OnPlayerLeft(CPlayerInfo cpiPlayer) {

        }

        public void OnServerName(string strServerName) {

        }

        public void OnTeamKillCountForKick(int iLimit) {

        }

        public void OnTeamKillValueIncrease(int iLimit) {

        }

        public void OnTeamKillValueDecreasePerSecond(int iLimit) {

        }

        public void OnTeamKillValueForKick(int iLimit) {

        }

        public void OnIdleTimeout(int iLimit) {

        }

        public void OnProfanityFilter(bool isEnabled) {

        }

        public void OnEndRound(int iWinningTeamID) {

        }

        public void OnRoundOverTeamScores(List<TeamScore> lstTeamScores) {

        }

        public void OnRoundOverPlayers(List<string> lstPlayers) {

        }

        public void OnRoundOver(int iWinningTeamID) {

        }

        public void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) {

        }

        public void OnLevelVariablesList(LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues) {

        }

        public void OnLevelVariablesEvaluate(LevelVariable lvRequestedContext, LevelVariable lvReturnedValue) {

        }

        public void OnLevelVariablesClear(LevelVariable lvRequestedContext) {

        }

        public void OnLevelVariablesSet(LevelVariable lvRequestedContext) {

        }

        public void OnLevelVariablesGet(LevelVariable lvRequestedContext, LevelVariable lvReturnedValue) {

        }

        #endregion

        #region IPRoConPluginInterface3

        //
        // IPRoConPluginInterface3
        //
        public void OnAnyMatchRegisteredCommand(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            
        }

        public void OnRegisteredCommand(MatchCommand mtcCommand) {

        }

        public void OnUnregisteredCommand(MatchCommand mtcCommand) {

        }

        public void OnZoneTrespass(CPlayerInfo cpiSoldier, ZoneAction action, MapZone sender, Point3D pntTresspassLocation, float flTresspassPercentage) {

        }

        public void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) {

        }

        public void OnMaplistList(List<MaplistEntry> lstMaplist) {

        }

        #endregion

        #region In Game Commands

        private void WriteBFBCsStats(string strScope, string strSpeaker, string targetPlayer, string statData) {

            string strPrefix = String.Format("bfbcs.com ({0}) > ", targetPlayer);

            List<string> wordWrappedLines = this.WordWrap(statData, 100 - strPrefix.Length);

            bool isAbleToGloballySpam = true;

            if (this.m_enAllowGeneralPublicToShoutDetails == enumBoolYesNo.No && this.GetAccountPrivileges(strSpeaker) == null) {
                isAbleToGloballySpam = false;
            }

            foreach (string line in wordWrappedLines) {

                if (String.Compare(strScope, "!") == 0 && isAbleToGloballySpam == true) {
                    this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("{0}{1}", strPrefix, line), "all");
                }
                else {
                    this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("{0}{1}", strPrefix, line), "player", strSpeaker);
                }
            }
        }

        private string GetStatsOnUnknownCategory(Hashtable player, string friendlyVariableName, bool showPlusStats) {

            string returnStats = "";

            List<string> lstCategories = this.Listify("gadgets", "weapons", "vehicles", "insiginias", "kits", "specializations", "pins", "favourites");

            foreach (string category in lstCategories) {
                try {
                    returnStats = PlayerInformation.HashtableStatsToString(PlayerInformation.GetTableByNameWithinCategory(player, category, friendlyVariableName), showPlusStats);
                }
                catch (Exception e) {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "BFBCSInGame.GetStatsOnUnknownCategory: " + e.Message);
                }
                if (returnStats.Length > 0) {
                    break;
                }
            }

            return returnStats;
        }

        public void OnCommandBfbcs(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (this.m_dicPlayers.ContainsKey(strSpeaker) == true) {
                this.m_dicPlayers[strSpeaker].FetchStats(this.OnCommandBfbcs_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
            }
        }

        //public void OnCommandBfbcsOnType(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
        //    if (this.m_dicPlayers.ContainsKey(strSpeaker) == true) {
        //        this.m_dicPlayers[strSpeaker].FetchStats(this.OnCommandBfbcsOnType_DownloadCallback, this.OnCommandBfbcs_FailureCallback, strSpeaker, capCommand);
        //    }
        //}

        public void OnCommandBfbcsOnPlayer(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            if (this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[0].Argument) == true) {
                this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].FetchStats(this.OnCommandBfbcsOnPlayer_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
            }
        }

        public void OnCommandBfbcsOnPlayerOnType(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {
            
            if (this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[0].Argument) == true) {
                this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].FetchStats(this.OnCommandBfbcsOnPlayerOnType_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
            }
        }


        public void OnCommandBfbcsOnComparePlayers(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (String.Compare(capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument) != 0) {

                if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].StatsRequireFetching == false && this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].StatsRequireFetching == false) {

                    // Skip ahead, both are available and don't need to be downloaded (or downloaded again).
                    this.OnCommandBfbcsOnComparePlayers_DownloadCallback(null, strSpeaker, capCommand);
                }
                else { // One or both of them needs to be downloaded.
                    if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].StatsRequireFetching == true) {
                        this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].FetchStats(this.OnCommandBfbcsOnComparePlayers_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
                    }

                    if (this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].StatsRequireFetching == true) {
                        this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].FetchStats(this.OnCommandBfbcsOnComparePlayers_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
                    }
                }
            }
            else {
                this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("bfbcs.com (*{0} vs {1}) > My head hurts =(", capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument), "player", strSpeaker);
            }
        }

        internal void OnCommandBfbcsOnComparePlayers_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            if (this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[0].Argument) == true && this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[2].Argument) == true) {

                if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].PlayerStats != null && this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].PlayerStats != null) {
                    Hashtable comparedPlayer = PlayerInformation.ComparePlayers(this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].PlayerStats, this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].PlayerStats);

                    this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, String.Format("*{0} vs {1}", capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument), PlayerInformation.HashtableStatsToString(comparedPlayer, true));
                }
            }
        }


        public void OnCommandBfbcsOnComparePlayersOnStat(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) {

            if (String.Compare(capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument) != 0) {

                if (this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[0].Argument) == true && this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[2].Argument) == true) {

                    if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].StatsRequireFetching == false && this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].StatsRequireFetching == false) {

                        // Skip ahead, both are available and don't need to be downloaded (or downloaded again).
                        this.OnCommandBfbcsOnComparePlayersOnStat_DownloadCallback(null, strSpeaker, capCommand);
                    }
                    else { // One or both of them needs to be downloaded.
                        if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].StatsRequireFetching == true) {
                            this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].FetchStats(this.OnCommandBfbcsOnComparePlayersOnStat_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
                        }

                        if (this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].StatsRequireFetching == true) {
                            this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].FetchStats(this.OnCommandBfbcsOnComparePlayersOnStat_DownloadCallback, this.OnCommandBfbcs_FailureCallback, this.OnCommandBfbcs_NoStatsAvailableCallback, strSpeaker, capCommand);
                        }
                    }
                }
            }
            else {
                this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("bfbcs.com (*{0} vs {1}) > My head hurts =(", capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument), "player", strSpeaker);
            }

            //this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("versing {0} versus {1} on {2}", capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument, capCommand.MatchedArguments[3].Argument), "all");
        }

        internal void OnCommandBfbcsOnComparePlayersOnStat_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {

            // Note: Race condition of both downloading and setting PlayerStats != null
            // avoided by lock inside of DownloadCompleted callback in PlayerInformation.

            
            if (this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[0].Argument) == true && this.m_dicPlayers.ContainsKey(capCommand.MatchedArguments[2].Argument) == true) {

                if (this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].PlayerStats != null && this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].PlayerStats != null) {
                    Hashtable comparedPlayer = PlayerInformation.ComparePlayers(this.m_dicPlayers[capCommand.MatchedArguments[0].Argument].PlayerStats, this.m_dicPlayers[capCommand.MatchedArguments[2].Argument].PlayerStats);

                    this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, String.Format("*{0} vs {1}", capCommand.MatchedArguments[0].Argument, capCommand.MatchedArguments[2].Argument), this.GetStatsOnUnknownCategory(comparedPlayer, capCommand.MatchedArguments[3].Argument, true));
                }
            }


        }

        internal void OnCommandBfbcs_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, strSpeaker, PlayerInformation.HashtableStatsToString(sender.PlayerStats, false));
        }

        //internal void OnCommandBfbcsOnType_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
        //    this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, this.GetStatsOnUnknownCategory(sender.PlayerStats, capCommand.MatchedArguments[0].Argument));
        //}

        internal void OnCommandBfbcsOnPlayer_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, capCommand.MatchedArguments[0].Argument, PlayerInformation.HashtableStatsToString(sender.PlayerStats, false));
        }

        internal void OnCommandBfbcsOnPlayerOnType_DownloadCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            this.WriteBFBCsStats(capCommand.ResposeScope, strSpeaker, capCommand.MatchedArguments[0].Argument, this.GetStatsOnUnknownCategory(sender.PlayerStats, capCommand.MatchedArguments[1].Argument, false));
        }

        internal void OnCommandBfbcs_FailureCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("bfbcs.com ({0}) > Error fetching stats or no stats available, please try again later.", sender.SoldierName), "player", strSpeaker);
        }

        internal void OnCommandBfbcs_NoStatsAvailableCallback(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand) {
            this.ExecuteCommand("procon.protected.send", "admin.say", String.Format("bfbcs.com ({0}) > No stats available for player, please try again later.", sender.SoldierName), "player", strSpeaker);
        }

        #endregion

        #region BFBCS internal player information

        internal class PlayerInformation : IDisposable {

            public delegate void StatsFetchedHandler(PlayerInformation sender, string strSpeaker, CapturedCommand capCommand);

            private StatsFetchedHandler m_delSuccess, m_delFailure, m_delNoStatsAvailable;
            private CapturedCommand m_capCommand;
            private string m_strSpeaker;

            private CPlayerInfo m_vanillaInfo;
            public CPlayerInfo VanillaInfo {
                get {
                    return this.m_vanillaInfo;
                }
                set {
                    this.m_vanillaInfo = value;
                }
            }

            private CPunkbusterInfo m_punkbusterInfo;
            public CPunkbusterInfo PunkbusterInfo {
                get {
                    return this.m_punkbusterInfo;
                }
                set {
                    this.m_punkbusterInfo = value;
                }
            }

            private Hashtable m_playerStats;
            public Hashtable PlayerStats {
                get {
                    return this.m_playerStats;
                }
                set {
                    this.m_playerStats = value;
                }
            }

            public string SoldierName {
                get {

                    string soldierName = String.Empty;

                    if (this.VanillaInfo != null) {
                        soldierName = this.VanillaInfo.SoldierName;
                    }
                    else if (this.PunkbusterInfo != null) {
                        soldierName = this.PunkbusterInfo.SoldierName;
                    }

                    return soldierName;
                }
            }

            public bool StatsRequireFetching {
                get {
                    return (this.PlayerStats == null);
                }
            }

            private CDownloadFile m_statFetcher;

            public PlayerInformation(CPlayerInfo vanillaInfo, CPunkbusterInfo punkbusterInfo, Hashtable playerStats) {
                this.VanillaInfo = vanillaInfo;
                this.PunkbusterInfo = punkbusterInfo;
                this.PlayerStats = playerStats;

                this.m_statFetcher = null;
            }

            public void FetchStats(StatsFetchedHandler delSuccess, StatsFetchedHandler delFailure, StatsFetchedHandler delNoStatsAvailable, string strSpeaker, CapturedCommand capCommand) {

                this.m_delSuccess = delSuccess;
                this.m_delFailure = delFailure;
                this.m_delNoStatsAvailable = delNoStatsAvailable;
                this.m_strSpeaker = strSpeaker;
                this.m_capCommand = capCommand;

                if (this.m_statFetcher == null && this.PlayerStats == null) {

                    if (this.SoldierName.Length > 0) {
                        this.m_statFetcher = new CDownloadFile(String.Format("http://api.bfbcs.com/api/pc?players={0}&fields=all", System.Uri.EscapeDataString(this.SoldierName)));
                        this.m_statFetcher.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(m_statFetcher_DownloadComplete);
                        this.m_statFetcher.DownloadError += new CDownloadFile.DownloadFileEventDelegate(m_statFetcher_DownloadError);

                        this.m_statFetcher.BeginDownload();
                    }
                }
                else if (this.PlayerStats != null) {
                    // Already have the stats, no need to nag the website..
                    if (this.m_delSuccess != null) {
                        this.m_delSuccess(this, this.m_strSpeaker, this.m_capCommand);
                    }
                }
            }

            public void Dispose() {

                if (this.m_statFetcher != null) {
                    this.m_statFetcher.EndDownload();
                }

            }

            private void m_statFetcher_DownloadError(CDownloadFile cdfSender) {
                // Null it so they can try again later..
                this.m_statFetcher.EndDownload();
                this.m_statFetcher = null;

                if (this.m_delFailure != null) {
                    this.m_delFailure(this, this.m_strSpeaker, this.m_capCommand);
                }
            }

            private readonly object m_objDownloadedLocker = new object();
            private void m_statFetcher_DownloadComplete(CDownloadFile cdfSender) {

                // Prevent race conditions if multiple files were downloaded for one command
                // and require both to have been downloaded to complete.
                lock (this.m_objDownloadedLocker) {
                    try {

                        this.PlayerStats = PlayerInformation.AdditionalPrepOnHashtable((Hashtable)JSON.JsonDecode(System.Text.Encoding.GetEncoding(1251).GetString(cdfSender.CompleteFileData)), this.SoldierName);

                        if (this.PlayerStats != null && this.m_delSuccess != null) {
                            this.m_delSuccess(this, this.m_strSpeaker, this.m_capCommand);
                        }
                        else if (this.PlayerStats == null && this.m_delNoStatsAvailable != null) {
							this.m_statFetcher = null;
							
                            this.m_delNoStatsAvailable(this, this.m_strSpeaker, this.m_capCommand);
                        }
                    }
                    catch (Exception e) {
                        this.m_statFetcher.EndDownload();
                        this.m_statFetcher = null;
                        
                        if (this.m_delFailure != null) {
                            this.m_delFailure(this, this.m_strSpeaker, this.m_capCommand);
                        }
                    }
                }
            }

            #region BFBCS JSON Hashtable data

            public static Hashtable GetPlayer(Hashtable data, string soldierName) {

                Hashtable returnTable = null;

                if (data != null && data.ContainsKey("players") == true) {
                    foreach (Hashtable player in (ArrayList)data["players"]) {
                        if (player.ContainsKey("name") == true && String.Compare((string)player["name"], soldierName) == 0) {
                            returnTable = player;
                            break;
                        }
                    }
                }

                return returnTable;
            }

            public static List<string> CompileNamesWithinArraylist(ArrayList subcategory) {

                List<string> returnNames = new List<string>();

                foreach (Hashtable table in subcategory) {
                    //foreach (DictionaryEntry category in table) {
                    if (table.ContainsKey("name") == true && returnNames.Contains((string)(table["name"])) == false) {
                        returnNames.Add((string)table["name"]);
                    }
                    //}
                }

                return returnNames;
            }

            public static List<string> CompileNamesWithinCategory(Hashtable locatedPlayer, string categoryName) {

                List<string> returnNames = new List<string>();

                if (locatedPlayer != null) {

                    if (locatedPlayer.ContainsKey(categoryName) == true) {

                        if (locatedPlayer[categoryName] is Hashtable) {

                            foreach (DictionaryEntry category in (Hashtable)locatedPlayer[categoryName]) {
                                if (((Hashtable)category.Value).ContainsKey("name") == true && returnNames.Contains((string)((Hashtable)category.Value)["name"]) == false) {
                                    returnNames.Add((string)((Hashtable)category.Value)["name"]);
                                }
                            }
                        }
                        else if (locatedPlayer[categoryName] is ArrayList) {
                            returnNames = PlayerInformation.CompileNamesWithinArraylist((ArrayList)locatedPlayer[categoryName]);
                        }
                    }
                }

                return returnNames;
            }

            public static Hashtable GetTableByNameWithinCategory(Hashtable locatedPlayer, string categoryName, string tableName) {

                Hashtable returnTable = null;

                if (locatedPlayer != null) {

                    if (locatedPlayer.ContainsKey(categoryName) == true) {

                        if (locatedPlayer[categoryName] is Hashtable) {

                            if (((Hashtable)locatedPlayer[categoryName]).ContainsKey("name") == true && String.Compare((string)((Hashtable)locatedPlayer[categoryName])["name"], tableName) == 0) {
                                returnTable = (Hashtable)locatedPlayer[categoryName];
                            }
                            else {
                                foreach (DictionaryEntry category in (Hashtable)locatedPlayer[categoryName]) {

                                    if (((Hashtable)category.Value).ContainsKey("name") == true && String.Compare((string)((Hashtable)category.Value)["name"], tableName) == 0) {
                                        returnTable = (Hashtable)category.Value;
                                    }
                                }
                            }
                        }
                        else if (locatedPlayer[categoryName] is ArrayList) {
                            foreach (Hashtable table in (ArrayList)locatedPlayer[categoryName]) {
                                if (table.ContainsKey("name") == true && String.Compare((string)(table["name"]), tableName) == 0) {
                                    returnTable = table;
                                    break;
                                }
                            }
                        }
                    }
                }

                return returnTable;
            }

            public static Hashtable GetHighestTableByCategoryVariable(Hashtable locatedPlayer, string categoryName, string variableName) {

                Hashtable returnTable = null;

                if (locatedPlayer != null) {

                    double highestVariable = 0.0D;

                    if (locatedPlayer.ContainsKey(categoryName) == true) {

                        if (locatedPlayer[categoryName] is Hashtable) {

                            foreach (DictionaryEntry category in (Hashtable)locatedPlayer[categoryName]) {

                                if (((Hashtable)category.Value)[variableName] is double && (double)((Hashtable)category.Value)[variableName] > highestVariable) {
                                    highestVariable = (double)((Hashtable)category.Value)[variableName];
                                    returnTable = (Hashtable)category.Value;
                                }
                            }
                        }
                    }
                }

                return returnTable;
            }

            internal class StatVariable<T> {

                public delegate string FormatStatVariableHandler(T value);

                private string m_name;
                public string Name {
                    get {
                        return this.m_name;
                    }
                    private set {
                        this.m_name = value;
                    }
                }

                private FormatStatVariableHandler m_formatStatVariable;
                public FormatStatVariableHandler FormatStatVariable {
                    get {
                        return this.m_formatStatVariable;
                    }
                    private set {
                        this.m_formatStatVariable = value;
                    }
                }

                public static K ConvertValue<K>(string value, K tDefault) {
                    K tReturn = tDefault;

                    TypeConverter tycPossible = TypeDescriptor.GetConverter(typeof(K));
                    if (value.Length > 0 && tycPossible.CanConvertFrom(typeof(string)) == true) {
                        tReturn = (K)tycPossible.ConvertFrom(value);
                    }
                    else {
                        tReturn = tDefault;
                    }

                    return tReturn;
                }

                public StatVariable(string name, FormatStatVariableHandler formatHandler) {

                    this.Name = name;
                    this.FormatStatVariable = formatHandler;

                }

                public string ToString(string value) {

                    string returnFormat = value;

                    if (this.FormatStatVariable != null) {
                        returnFormat = this.FormatStatVariable(StatVariable<T>.ConvertValue(value, default(T)));
                    }

                    return returnFormat;
                }
            }

            public static string FormatSeconds(double seconds) {

                TimeSpan length = TimeSpan.FromSeconds(seconds);
                length = new TimeSpan(length.Days, length.Hours, length.Minutes, length.Seconds);

                string returnFormat = length.ToString();

                if (length.Days > 0) {
                    returnFormat = String.Format("{0} days", returnFormat);
                }
                else {
                    returnFormat = String.Format("{0} hours", returnFormat);
                }

                return returnFormat;
            }

            public static string FormatShots(double shots) {
                return String.Format("{0:0} shots", shots);
            }

            public static string FormatHits(double hits) {
                return String.Format("{0:0} hits", hits);
            }

            public static string FormatAccuracy(double accuracy) {
                return String.Format("{0:0.0}% accuracy", accuracy * 100.0D);
            }

            public static string FormatHeadshots(double headshots) {
                return String.Format("{0:0} headshots", headshots);
            }

            public static string FormatKills(double kills) {
                return String.Format("{0:0} kills", kills);
            }

            public static string FormatDeaths(double deaths) {
                return String.Format("{0:0} deaths", deaths);
            }

            public static string FormatKdr(double kdr) {
                return String.Format("{0:0.00} kdr", kdr);
            }

            public static string FormatElo(double elo) {
                return String.Format("{0:0.00} elo", elo);
            }

            public static string FormatScore(double score) {
                return String.Format("{0:0} score", score);
            }

            public static string FormatDistance(double distance) {
                return String.Format("{0:0.00} km's", distance / 1000);
            }

            public static string FormatRoadkills(double roadkills) {
                return String.Format("{0:0} roadkills", roadkills);
            }

            public static string FormatRevives(double revives) {
                return String.Format("{0:0} revives", revives);
            }

            public static string FormatRepairs(double repairs) {
                return String.Format("{0:0} repairs", repairs);
            }

            public static string FormatHeals(double heals) {
                return String.Format("{0:0} heals", heals);
            }

            public static string FormatResupplies(double resupplies) {
                return String.Format("{0:0} resupplies", resupplies);
            }

            public static string FormatValue(double value) {
                return String.Format("{0:0} of", value);
            }

            public static string FormatTarget(double target) {
                return String.Format("{0:0} target", target);
            }

            public static string FormatCount(double count) {
                return String.Format("recieved {0:0} times", count);
            }

            public static string HashtableStatsToString(Hashtable parsedStatsTable, bool showPlusStats) {

                string returnStats = "";

                if (parsedStatsTable != null) {

                    Hashtable statsTable = new Hashtable(parsedStatsTable);

                    // "favourites", "favouritevehicle", "favouritegadget", "favouriteweapon", "favouritekit",
                    List<string> variableOrder = new List<string>();
                    variableOrder.Add("name");
                    variableOrder.Add("score");
                    variableOrder.Add("elo");
                    variableOrder.Add("kills");
                    variableOrder.Add("roadkills");
                    variableOrder.Add("deaths");
                    variableOrder.Add("kdr");
                    variableOrder.Add("headshots");
                    variableOrder.Add("shots_fired");
                    variableOrder.Add("shots_hit");
                    variableOrder.Add("accuracy");
                    variableOrder.Add("distance");
                    variableOrder.Add("time");
                    variableOrder.Add("seconds");
                    variableOrder.Add("heals");
                    variableOrder.Add("repairs");
                    variableOrder.Add("revives");
                    variableOrder.Add("resupplies");
                    variableOrder.Add("criteria1");
                    variableOrder.Add("label");
                    variableOrder.Add("value");
                    variableOrder.Add("target");
                    variableOrder.Add("count");

                    //variableOrder.Add("favourites");
                    variableOrder.Add("favouritevehicle");
                    variableOrder.Add("favouritegadget");
                    variableOrder.Add("favouriteweapon");
                    variableOrder.Add("favouritekit");

                    Dictionary<string, StatVariable<double>> numberFormatter = new Dictionary<string, StatVariable<double>>();
                    numberFormatter.Add("seconds", new StatVariable<double>("seconds", PlayerInformation.FormatSeconds));
                    numberFormatter.Add("time", new StatVariable<double>("time", PlayerInformation.FormatSeconds));

                    numberFormatter.Add("shots_fired", new StatVariable<double>("shots_fired", PlayerInformation.FormatShots));
                    numberFormatter.Add("shots_hit", new StatVariable<double>("shots_hit", PlayerInformation.FormatHits));
                    numberFormatter.Add("accuracy", new StatVariable<double>("accuracy", PlayerInformation.FormatAccuracy));

                    numberFormatter.Add("headshots", new StatVariable<double>("headshots", PlayerInformation.FormatHeadshots));

                    numberFormatter.Add("kills", new StatVariable<double>("kills", PlayerInformation.FormatKills));
                    numberFormatter.Add("deaths", new StatVariable<double>("deaths", PlayerInformation.FormatDeaths));
                    numberFormatter.Add("kdr", new StatVariable<double>("kdr", PlayerInformation.FormatKdr));

                    numberFormatter.Add("elo", new StatVariable<double>("elo", PlayerInformation.FormatElo));
                    numberFormatter.Add("score", new StatVariable<double>("score", PlayerInformation.FormatScore));

                    numberFormatter.Add("distance", new StatVariable<double>("distance", PlayerInformation.FormatDistance));
                    numberFormatter.Add("roadkills", new StatVariable<double>("roadkills", PlayerInformation.FormatRoadkills));

                    numberFormatter.Add("heals", new StatVariable<double>("heals", PlayerInformation.FormatHeals));
                    numberFormatter.Add("repairs", new StatVariable<double>("repairs", PlayerInformation.FormatRepairs));
                    numberFormatter.Add("revives", new StatVariable<double>("revives", PlayerInformation.FormatRevives));
                    numberFormatter.Add("resupplies", new StatVariable<double>("resupplies", PlayerInformation.FormatResupplies));

                    numberFormatter.Add("value", new StatVariable<double>("value", PlayerInformation.FormatValue));
                    numberFormatter.Add("target", new StatVariable<double>("target", PlayerInformation.FormatTarget));
                    numberFormatter.Add("count", new StatVariable<double>("count", PlayerInformation.FormatCount));

                    if (statsTable != null) {

                        string seperator = "";

                        foreach (string variable in variableOrder) {

                            if (statsTable.ContainsKey(variable) == true) {

                                if (numberFormatter.ContainsKey(variable) == true) {

                                    if (showPlusStats == true && statsTable[variable] is Double && (Double)statsTable[variable] > 0.0D) {
                                        returnStats += String.Format("{0}+{1}", seperator, numberFormatter[variable].ToString(statsTable[variable].ToString()));
                                    }
                                    else {
                                        returnStats += String.Format("{0}{1}", seperator, numberFormatter[variable].ToString(statsTable[variable].ToString()));
                                    }

                                    seperator = ", ";
                                }
                                else if (statsTable[variable] is Hashtable) {
                                    returnStats += String.Format("{0}{1}", seperator, PlayerInformation.HashtableStatsToString((Hashtable)statsTable[variable], showPlusStats));
                                    seperator = ", ";
                                }
                                else if (statsTable[variable] is string) {
                                    returnStats += String.Format("{0}: ", statsTable[variable]);
                                }
                            }

                        }
                    }
                }

                return returnStats;
            }

            public static ArrayList PrepPlayer(ArrayList player) {
                ArrayList returnPlayer = new ArrayList(player);

                for (int i = 0; i < returnPlayer.Count; i++) {
                    if (returnPlayer[i] is Hashtable) {
                        returnPlayer[i] = PlayerInformation.PrepPlayer((Hashtable)returnPlayer[i]);
                    }
                }

                return returnPlayer;
            }

            public static Hashtable PrepPlayer(Hashtable player) {

                Hashtable returnPlayer = new Hashtable(player);

                foreach (DictionaryEntry entry in player) {

                    if (entry.Value is Hashtable) {
                        returnPlayer[entry.Key] = PlayerInformation.PrepPlayer((Hashtable)returnPlayer[entry.Key]);
                    }
                    else if (entry.Value is ArrayList) {
                        returnPlayer[entry.Key] = PlayerInformation.PrepPlayer((ArrayList)returnPlayer[entry.Key]);
                    }
                }

                if (returnPlayer.ContainsKey("shots_fired") == true && returnPlayer.ContainsKey("shots_hit") == true) {

                    double shotsHit = StatVariable<double>.ConvertValue(returnPlayer["shots_hit"].ToString(), 0.0D);
                    double shotsFired = StatVariable<double>.ConvertValue(returnPlayer["shots_fired"].ToString(), 0.0D);

                    if (shotsFired > 0.0D) {
                        returnPlayer.Add("accuracy", shotsHit / shotsFired);
                    }
                    else {
                        returnPlayer.Add("accuracy", 0.0D);
                    }
                }

                if (returnPlayer.ContainsKey("kills") == true && returnPlayer.ContainsKey("deaths") == true) {

                    double kills = StatVariable<double>.ConvertValue(returnPlayer["kills"].ToString(), 0.0D);
                    double deaths = StatVariable<double>.ConvertValue(returnPlayer["deaths"].ToString(), 0.0D);

                    if (deaths > 0.0D) {
                        returnPlayer.Add("kdr", kills / deaths);
                    }
                    else {
                        returnPlayer.Add("kdr", kills);
                    }
                }

                return returnPlayer;
            }

            public static Hashtable AdditionalPrepOnHashtable(Hashtable data, string soldierName) {

                Hashtable returnPlayer = PlayerInformation.GetPlayer(data, soldierName);

				if (returnPlayer != null) {

					returnPlayer = PlayerInformation.PrepPlayer(returnPlayer);

					Hashtable favouriteVehicle = PlayerInformation.GetHighestTableByCategoryVariable(returnPlayer, "vehicles", "kills");
					Hashtable favouriteGadget = PlayerInformation.GetHighestTableByCategoryVariable(returnPlayer, "gadgets", "seconds");
					Hashtable favouriteWeapon = PlayerInformation.GetHighestTableByCategoryVariable(returnPlayer, "weapons", "kills");
					Hashtable favouriteKit = PlayerInformation.GetHighestTableByCategoryVariable(returnPlayer, "kits", "score");

					Hashtable favourites = new Hashtable();

					favourites.Add("name", "Favourites");

					if (favouriteVehicle != null) {
						Hashtable favouritevehicle = new Hashtable();
						favouritevehicle.Add("name", favouriteVehicle["name"]);
						favouritevehicle.Add("kills", favouriteVehicle["kills"]);
						favourites.Add("favouritevehicle", favouritevehicle);
					}

					if (favouriteGadget != null) {
						Hashtable favouritegadget = new Hashtable();
						favouritegadget.Add("name", favouriteGadget["name"]);
						favouritegadget.Add("seconds", favouriteGadget["seconds"]);
						favourites.Add("favouritegadget", favouritegadget);
					}

					if (favouriteWeapon != null) {
						Hashtable favouriteweapon = new Hashtable();
						favouriteweapon.Add("name", favouriteWeapon["name"]);
						favouriteweapon.Add("kills", favouriteWeapon["kills"]);
						favourites.Add("favouriteweapon", favouriteweapon);
					}

					if (favouriteKit != null) {
						Hashtable favouritekit = new Hashtable();
						favouritekit.Add("name", favouriteKit["name"]);
						favouritekit.Add("score", favouriteKit["score"]);
						favouritekit.Add("kills", favouriteKit["kills"]);
						favouritekit.Add("deaths", favouriteKit["deaths"]);
						favourites.Add("favouritekit", favouritekit);
					}

					returnPlayer.Add("favourites", favourites);

				}

                return returnPlayer;
            }

            public static ArrayList ComparePlayers(ArrayList player1, ArrayList player2) {
                ArrayList returnPlayer = new ArrayList(player1);
                int minimumCount = (int)Math.Min(returnPlayer.Count, player2.Count);

                for (int i = 0; i < minimumCount; i++) {

                    if (returnPlayer[i] is Hashtable && player2[i] is Hashtable) {
                        returnPlayer[i] = PlayerInformation.ComparePlayers((Hashtable)returnPlayer[i], (Hashtable)player2[i]);
                    }
                    else if (returnPlayer[i] is Double) {
                        returnPlayer[i] = (Double)returnPlayer[i] - (Double)player2[i];
                    }
                }

                return returnPlayer;
            }

            public static Hashtable ComparePlayers(Hashtable player1, Hashtable player2) {

                Hashtable returnPlayer = new Hashtable(player1);

                foreach (DictionaryEntry entry in player1) {
                    if (player2.ContainsKey(entry.Key) == true) {

                        if (entry.Value is Hashtable) {
                            returnPlayer[entry.Key] = PlayerInformation.ComparePlayers((Hashtable)returnPlayer[entry.Key], (Hashtable)player2[entry.Key]);
                        }
                        else if (entry.Value is ArrayList) {
                            returnPlayer[entry.Key] = PlayerInformation.ComparePlayers((ArrayList)returnPlayer[entry.Key], (ArrayList)player2[entry.Key]);
                        }
                        else if (returnPlayer[entry.Key] is Double && player2[entry.Key] is Double) {
                            returnPlayer[entry.Key] = (Double)returnPlayer[entry.Key] - (Double)player2[entry.Key];
                        }
                    }
                }

                return returnPlayer;
            }

            #endregion
        }

        #endregion

        #region Unused interface implementations

        // Account created
        public void OnAccountCreated(string strUsername) {

        }

        public void OnAccountDeleted(string strUsername) {

        }

        public void OnAccountPrivilegesUpdate(string strUsername, CPrivileges cpPrivs) {

        }

        public void OnReceiveProconVariable(string strVariableName, string strValue) {

        }

        // Connection
        public void OnConnectionClosed() {

        }

        public void OnPlayerAuthenticated(string strSoldierName, string strGuid) {

        }

        public void OnPlayerKilled(string strKillerSoldierName, string strVictimSoldierName) {

        }

        // Will receive ALL chat global/team/squad in R3.
        public void OnGlobalChat(string strSpeaker, string strMessage) {

        }

        // Place holder, non-functioning in R3.  It recieves the same data as OnGlobalChat though so look out for now.
        public void OnTeamChat(string strSpeaker, string strMessage, int iTeamID) {

        }

        // Place holder, non-functioning in R3.  It recieves the same data as OnGlobalChat though so look out for now.
        public void OnSquadChat(string strSpeaker, string strMessage, int iTeamID, int iSquadID) {

        }

        public void OnLoadingLevel(string strMapFileName) {

        }

        public void OnLevelStarted() {

        }

        public void OnPunkbusterMessage(string strPunkbusterMessage) {

        }

        public void OnPunkbusterBanInfo(CBanInfo cbiPunkbusterBan) {

        }

        // Global or misc..
        public void OnResponseError(List<string> lstRequestWords, string strError) {

        }

        // Login events
        public void OnLogin() {

        }

        public void OnLogout() {

        }

        public void OnQuit() {

        }

        public void OnVersion(string strServerType, string strVersion) {

        }

        public void OnHelp(List<string> lstCommands) {

        }

        public void OnRunScript(string strScriptFileName) {

        }

        public void OnRunScriptError(string strScriptFileName, int iLineError, string strErrorDescription) {

        }

        // Query Events
        public void OnServerInfo(CServerInfo csiServerInfo) {

        }

        // Communication Events
        public void OnYelling(string strMessage, int iMessageDuration, CPlayerSubset cpsSubset) {

        }

        public void OnSaying(string strMessage, CPlayerSubset cpsSubset) {

        }

        // Level Events
        public void OnRunNextLevel() {

        }

        public void OnCurrentLevel(string strCurrentLevel) {

        }

        public void OnSetNextLevel(string strNextLevel) {

        }

        public void OnRestartLevel() {

        }

        // Does not work in R3, never called for now.
        public void OnSupportedMaps(string strPlayList, List<string> lstSupportedMaps) {

        }

        public void OnPlaylistSet(string strPlaylist) {

        }

        public void OnListPlaylists(List<string> lstPlaylists) {

        }


        // Player Kick/List Events
        public void OnPlayerKicked(string strSoldierName, string strReason) {

        }

        public void OnPlayerTeamChange(string strSoldierName, int iTeamID, int iSquadID) {

        }

        public void OnPlayerSquadChange(string strSpeaker, int iTeamID, int iSquadID) {

        }

        // Banning and Banlist Events
        public void OnBanList(List<CBanInfo> lstBans) {

        }

        public void OnBanAdded(CBanInfo cbiBan) {

        }

        public void OnBanRemoved(CBanInfo cbiUnban) {

        }

        public void OnBanListClear() {

        }

        public void OnBanListLoad() {

        }

        public void OnBanListSave() {

        }

        // Reserved Slots Events
        public void OnReservedSlotsConfigFile(string strConfigFilename) {

        }

        public void OnReservedSlotsLoad() {

        }

        public void OnReservedSlotsSave() {

        }

        public void OnReservedSlotsPlayerAdded(string strSoldierName) {

        }

        public void OnReservedSlotsPlayerRemoved(string strSoldierName) {

        }

        public void OnReservedSlotsCleared() {

        }

        public void OnReservedSlotsList(List<string> lstSoldierNames) {

        }

        // Maplist Events
        public void OnMaplistConfigFile(string strConfigFilename) {

        }

        public void OnMaplistLoad() {

        }

        public void OnMaplistSave() {

        }

        public void OnMaplistMapAppended(string strMapFileName) {

        }

        public void OnMaplistMapRemoved(int iMapIndex) {

        }

        public void OnMaplistCleared() {

        }

        public void OnMaplistList(List<string> lstMapFileNames) {

        }

        public void OnMaplistNextLevelIndex(int iMapIndex) {

        }

        public void OnMaplistMapInserted(int iMapIndex, string strMapFileName) {

        }

        // Vars
        public void OnGamePassword(string strGamePassword) {

        }

        public void OnPunkbuster(bool blEnabled) {

        }

        public void OnHardcore(bool blEnabled) {

        }

        public void OnRanked(bool blEnabled) {

        }

        public void OnRankLimit(int iRankLimit) {

        }

        public void OnTeamBalance(bool blEnabled) {

        }

        public void OnFriendlyFire(bool blEnabled) {

        }

        public void OnMaxPlayerLimit(int iMaxPlayerLimit) {

        }

        public void OnCurrentPlayerLimit(int iCurrentPlayerLimit) {

        }

        public void OnPlayerLimit(int iPlayerLimit) {

        }

        public void OnBannerURL(string strURL) {

        }

        public void OnServerDescription(string strServerDescription) {

        }

        public void OnKillCam(bool blEnabled) {

        }

        public void OnMiniMap(bool blEnabled) {

        }

        public void OnCrossHair(bool blEnabled) {

        }

        public void On3dSpotting(bool blEnabled) {

        }

        public void OnMiniMapSpotting(bool blEnabled) {

        }

        public void OnThirdPersonVehicleCameras(bool blEnabled) {

        }

        #endregion
    }

}