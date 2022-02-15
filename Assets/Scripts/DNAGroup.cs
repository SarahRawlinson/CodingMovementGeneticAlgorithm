using System;
using UnityEngine;

[Serializable]
    public class DNAGroup
    {
        public DNA movementDnaForwardBackward;
        public DNA heightDna;
        public DNA movementDnaLeftRight;
        public DNA movementDnaTurn;
        public DNA colourDna;
        public DNA priorityDna;

        public DNAGroup CopyGeneGroupStructure() => new DNAGroup() {
            movementDnaForwardBackward = DNA.CopyType(movementDnaForwardBackward, false),
            heightDna = DNA.CopyType(heightDna, false),
            movementDnaLeftRight = DNA.CopyType(movementDnaLeftRight, false),
            movementDnaTurn = DNA.CopyType(movementDnaTurn, false),
            priorityDna = DNA.CopyType(priorityDna, false),
            colourDna = DNA.CopyType(colourDna, false)
        };

        static public DNAGroup Clone(DNAGroup dndGroupOriginal)
        {
            DNAGroup dnaGroupCopy = new DNAGroup();
            dnaGroupCopy.movementDnaForwardBackward = DNA.CopyType(dndGroupOriginal.movementDnaForwardBackward, true);
            dnaGroupCopy.heightDna = DNA.CopyType(dndGroupOriginal.heightDna, true);
            dnaGroupCopy.movementDnaLeftRight = DNA.CopyType(dndGroupOriginal.movementDnaLeftRight, true);
            dnaGroupCopy.movementDnaTurn = DNA.CopyType(dndGroupOriginal.movementDnaTurn, true);
            dnaGroupCopy.priorityDna = DNA.CopyType(dndGroupOriginal.priorityDna, true);
            dnaGroupCopy.colourDna = DNA.CopyType(dndGroupOriginal.colourDna, true);
            return dnaGroupCopy;
        }
        public static void PrintDnaColour(DNAGroup dnaGroup)
        {
            float r = dnaGroup.colourDna.genes[0] / 100f;
            float g = dnaGroup.colourDna.genes[1] / 100f;
            float b = dnaGroup.colourDna.genes[2] / 100f;
            Color dnaColour = new Color(r, g, b);
            // Debug.Log($"<color=dnaColour>r{r} :  g:{g}  b:{b} </color>");
            Debug.Log (string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(dnaColour.r * 255f), (byte)(dnaColour.g * 255f), (byte)(dnaColour.b * 255f), $"r{r} :  g:{g}  b:{b}"));
        }
    }