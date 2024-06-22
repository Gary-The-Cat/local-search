﻿using Game.Factories;
using Game.Helpers;
using Game.SFML_Text;
using SFML.Graphics;
using System.Collections.Generic;
using Game.GeneticAlgorithm;
using Game.ViewTools;
using Game.ExtensionMethods;
using SFML.System;

namespace Game.Screens
{
    public class SimulationScreen : Screen
    {
        private readonly List<ConvexShape> pathLines;

        private static Vector2f totalDistanceStringPosition = new Vector2f(SimulationConfig.Width / 2, 50);

        private static Vector2f generationStringPosition = new Vector2f(50, 50);

        private static Vector2f quitStringPosition = new Vector2f(450, SimulationConfig.Height - 100);

        private List<RectangleShape> townVisuals;

        private FontText totalDistanceString;

        private FontText quitString;

        private object pathLock = new object();

        private object distanceStringLock = new object();

        private FontText generationString;
        public FontText GenerationString 
        {
            get => generationString;
            set => generationString = value;
        }

        public SimulationScreen(
            RenderWindow window,
            FloatRect configuration)
            : base(window, configuration)
        {
            this.townVisuals = new List<RectangleShape>();
            this.pathLines = new List<ConvexShape>();

            // Grab the images that are used to represent our towns and insert them into quads so they can be shown.
            // For now our town positions are hard coded, but theres no reason we cant expand this in future to randomly place them.
            TownFactory.GetTowns().ForEach(t => this.townVisuals.Add(t.Shape));

            // Create a camera instance to handle the changing of window sizes
            Camera = new Camera(SimulationConfig.SinglePlayer);

            // Create a 'FontText' which is a simple wrapper to easily draw text to the screen.
            totalDistanceString = new FontText(new Font("Resources/font.ttf"), string.Empty, Color.Black, 3);
            GenerationString = new FontText(new Font("Resources/font.ttf"), $"Generation: {0}", Color.Black, 3);
            quitString = new FontText(new Font("Resources/font.ttf"), "Press 'Q' to quit.", Color.Black, 3);
        }

        /// <summary>
        /// Updates the currently visualised sequence
        /// </summary>
        /// <param name="individual">The new sequence to display on the map</param>
        public void UpdateSequence(List<ConvexShape> path)
        {
            // Convert our sequence of ints to the 2D line representations to be drawn on the screen.
            lock (pathLock)
            {
                pathLines.Clear();
                pathLines.AddRange(path);
            }
        }

        /// <summary>
        /// Update - Update each of the components that are time dependent.
        /// </summary>
        /// <param name="deltaT"></param>
        public override void Update(float deltaT)
        {
            base.Update(deltaT);
        }

        /// <summary>
        /// Draw - Here we don't update any of the components, only draw them in their current state to the screen.
        /// </summary>
        public override void Draw(float deltaT)
        {
            // Draw all of our towns
            townVisuals.ForEach(t => window.Draw(t));

            // Draw each of the line segment for the path we are currently displaying
            lock (pathLock)
            {
                pathLines.ForEach(p => window.Draw(p));
            }
        }

        /// <summary>
        /// We currently only set the text to green to indicate we have stopped improving but
        /// this should probably update some internal state.
        /// </summary>
        public void SetGACompleted()
        {
            totalDistanceString.TextColour = Color.Green;
        }
    }
}