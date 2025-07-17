using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

public class LightsPlugin
{
   // Mock data for the lights
   private readonly List<LightModel> lights = new()
   {
      new LightModel { Id = 1, Name = "Main Stage", IsOn = false, IsBlinking = false, Brightness = Brightness.Medium, Color = "#FFFFFF" },
      new LightModel { Id = 2, Name = "Second Stage", IsOn = false, IsBlinking = true, Brightness = Brightness.High, Color = "#FF0000" },
      new LightModel { Id = 3, Name = "Outside", IsOn = false, IsBlinking = false, Brightness = Brightness.Low, Color = "#FFFF00"  },
      new LightModel { Id = 4, Name = "Entrance", IsOn = true, IsBlinking = false, Brightness = Brightness.Low, Color = "#FFFF00"  },
   };

   [KernelFunction("get_lights")]
   [Description("Gets a list of lights and their current state")]
   public async Task<List<LightModel>> GetLightsAsync()
   {
      return lights;
   }

   [KernelFunction("change_state")]
   [Description("Changes the state of the light")]
   public async Task<LightModel?> ChangeStateAsync(int id, bool isOn)
   {
      var light = lights.FirstOrDefault(light => light.Id == id);

      if (light == null)
      {
         return null;
      }

      // Update the light with the new state
      light.IsOn = isOn;

      return light;
   }

   [KernelFunction("change_blinking")]
   [Description("Changes the blinking state of the light")]
   public async Task<LightModel?> ChangeBlinkingAsync(int id, bool isBlinking)
   { 
      var light = lights.FirstOrDefault(light => light.Id == id);

      if (light == null)
      {
         return null;
      }

      // Update the light with the new state
      light.IsBlinking = isBlinking;

      return light;
   } 


   [KernelFunction("change_color")]
   [Description("Changes the color of the light by passing the RGB color hex code")]
   public async Task<LightModel?> ChangeColorAsync(int id, string rgbColor)
   {
      var light = lights.FirstOrDefault(light => light.Id == id);

      if (light == null)
      {
         return null;
      }

      // Update the light with the new state
      light.Color = rgbColor;

      return light;
   }

   [KernelFunction("change_brightness")]
   [Description("Changes the brightness value of the light")]
   public async Task<LightModel?> ChangeBrightnessAsync(int id, Brightness brightness)
   {
      var light = lights.FirstOrDefault(light => light.Id == id);

      if (light == null)
      {
         return null;
      }

      // Update the light with the new state
      light.Brightness = brightness;

      return light;
   }
}

public class LightModel
{
   [JsonPropertyName("id")]
   public int Id { get; set; }

   [JsonPropertyName("name")]
   public required string Name { get; set; }

   [JsonPropertyName("is_on")]
   public bool? IsOn { get; set; }

   [JsonPropertyName("brightness")]
   public Brightness? Brightness { get; set; }

   [JsonPropertyName("color")]
   [Description("The color of the light with a hex code (ensure you include the # symbol)")]
   public string? Color { get; set; }
   
   [JsonPropertyName("is_blinking")]
   public bool? IsBlinking { get; set; } 

}


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Brightness
{
   Low,
   Medium,
   High
}