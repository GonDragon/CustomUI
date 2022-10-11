using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomUI
{
    [StaticConstructorOnStartup]
    internal static class Textures
    {
        public static readonly Texture2D toggleIcon = ContentFinder<Texture2D>.Get("UI/Playsettings/gd-editMode");
        public static readonly Texture2D configIcon = ContentFinder<Texture2D>.Get("UI/Icons/gd-cog");
    }
}
