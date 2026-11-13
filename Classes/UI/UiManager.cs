using ImGuiNET;
using System.Numerics;

namespace _3D_Engine.Classes.UI
{
    public enum UiTypes
    {
        MainMenu,
        Simulation,

    }

    public static class UiManager
    {
        public static ImGuiController guiController;
        public static Dictionary<string, string> UiTexts = new Dictionary<string, string>();
        public static UiTypes UiMode;

        public static List<float> FitnessGraphValues = new List<float>() { 0};

        public static void OnLoad()
        {
            //ImGui.SetNextWindowBgAlpha(1f);
        }

        public static void MainMenu()
        {

        }

        public static void SimulationUI()
        {

            ImGui.Begin("Creature Editor");
            if (ImGui.Button("Save")) { /* ... */ }
            //ImGui.SliderFloat("Stiffness", ref stiffness, 0f, 200f);
            //ImGui.Text($"Generation: ");
            foreach (var text in UiTexts)
            {
                ImGui.Text($"{text.Key}: {text.Value}");
            }
            ImGui.PlotLines(
                label: "Best Fitness",
                values: ref FitnessGraphValues.ToArray()[0],
                values_count: FitnessGraphValues.Count,
                values_offset: 0,
                overlay_text: "",
                scale_min: -1.0f,
                scale_max: 1.0f,
                graph_size: new Vector2(0, 80) // 0 width fills available window space
                                   );

            ImGui.End();



        }

        public static void Update()
        {
            switch (UiMode)
            {
                case UiTypes.Simulation:
                    SimulationUI();
                    break;
            }

            
        }
    }
}
