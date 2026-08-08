using System.Collections.Generic;
using UnityEngine;

namespace AudioTool {
    /// <summary>
    /// Renders audio waveforms to textures for visualization in the editor.
    /// Stateless for textures - caller owns and passes texture references.
    /// </summary>
    public class AudioWaveformRenderer {
        private Color _backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private Color _waveformColor = new Color(0.3f, 0.7f, 1f, 1f);
        private Color _selectionColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
        private Color _playheadColor = new Color(1f, 0.3f, 0.3f, 1f);
        private Color _centerLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        /// <summary>
        /// Gets or sets the background color of the waveform.
        /// </summary>
        public Color BackgroundColor {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the waveform color.
        /// </summary>
        public Color WaveformColor {
            get => _waveformColor;
            set => _waveformColor = value;
        }

        /// <summary>
        /// Gets or sets the selection highlight color.
        /// </summary>
        public Color SelectionColor {
            get => _selectionColor;
            set => _selectionColor = value;
        }

        /// <summary>
        /// Renders a waveform from an AudioClip to a texture.
        /// </summary>
        /// <param name="texture">Reference to texture to render into (will be created/resized if needed)</param>
        /// <param name="clip">The audio clip to visualize</param>
        /// <param name="width">Width of the output texture</param>
        /// <param name="height">Height of the output texture</param>
        /// <returns>True if successful</returns>
        public bool RenderWaveform(ref Texture2D texture, AudioClip clip, int width, int height) {
            if (clip == null) {
                return false;
            }

            if (!clip.LoadAudioData()) {
                Debug.LogWarning("Could not load audio data for waveform rendering");
                return false;
            }

            var samples = new float[clip.samples * clip.channels];
            if (!clip.GetData(samples, 0)) {
                Debug.LogWarning("Could not retrieve audio samples");
                return false;
            }

            return RenderWaveform(ref texture, samples, clip.channels, width, height);
        }

        /// <summary>
        /// Renders a waveform from raw samples to a texture.
        /// </summary>
        /// <param name="texture">Reference to texture to render into (will be created/resized if needed)</param>
        /// <param name="samples">Interleaved audio samples</param>
        /// <param name="channels">Number of audio channels</param>
        /// <param name="width">Width of the output texture</param>
        /// <param name="height">Height of the output texture</param>
        /// <returns>True if successful</returns>
        public bool RenderWaveform(ref Texture2D texture, float[] samples, int channels, int width, int height) {
            if (samples == null || samples.Length == 0) {
                return false;
            }

            if (channels <= 0) {
                channels = 1;
            }

            if (width <= 0 || height <= 0) {
                return false;
            }

            if (texture == null || texture.width != width || texture.height != height) {
                if (texture != null) {
                    Object.DestroyImmediate(texture);
                }
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++) {
                pixels[i] = _backgroundColor;
            }

            var sectionHeight = Mathf.Max(1, height / channels);
            for (var ch = 0; ch < channels; ch++) {
                var centerY = (ch * sectionHeight) + (sectionHeight / 2);
                if (centerY >= 0 && centerY < height) {
                    for (var x = 0; x < width; x++) {
                        var pixelIndex = (centerY * width) + x;
                        if (pixelIndex >= 0 && pixelIndex < pixels.Length) {
                            pixels[pixelIndex] = _centerLineColor;
                        }
                    }
                }
            }

            var channelSamples = GetChannelSamples(samples, channels);
            for (var ch = 0; ch < channels && ch < channelSamples.Count; ch++) {
                var channelMinY = ch * sectionHeight;
                var channelMaxY = Mathf.Min(((ch + 1) * sectionHeight) - 1, height - 1);
                if (channelSamples[ch] != null && channelSamples[ch].Count > 0) {
                    DrawChannel(pixels, width, channelSamples[ch], channelMinY, channelMaxY);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return true;
        }

        /// <summary>
        /// Creates an overlay texture with selection and playhead.
        /// </summary>
        /// <param name="texture">Reference to texture to render into (will be created/resized if needed)</param>
        /// <param name="width">Width of the overlay</param>
        /// <param name="height">Height of the overlay</param>
        /// <param name="selectionStart">Normalized start position of selection (0-1)</param>
        /// <param name="selectionEnd">Normalized end position of selection (0-1)</param>
        /// <param name="playheadPosition">Normalized playhead position (0-1), negative to hide</param>
        /// <param name="silenceStart">Normalized start of detected silence region (0-1)</param>
        /// <param name="silenceEnd">Normalized end of detected silence region (0-1)</param>
        /// <param name="fadeInEnd">Normalized position where fade-in ends (-1 to disable)</param>
        /// <param name="fadeOutStart">Normalized position where fade-out starts (-1 to disable)</param>
        /// <param name="fadeInCurve">AnimationCurve for fade-in visualization (null to skip)</param>
        /// <param name="fadeOutCurve">AnimationCurve for fade-out visualization (null to skip)</param>
        /// <returns>True if successful</returns>
        public bool CreateOverlay(ref Texture2D texture, int width, int height, float selectionStart, float selectionEnd,
            float playheadPosition = -1f, float silenceStart = 0f, float silenceEnd = 1f,
            float fadeInEnd = -1f, float fadeOutStart = -1f,
            AnimationCurve fadeInCurve = null, AnimationCurve fadeOutCurve = null) {
            if (width <= 0 || height <= 0) {
                return false;
            }

            if (texture == null || texture.width != width || texture.height != height) {
                if (texture != null) {
                    Object.DestroyImmediate(texture);
                }
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            var fadeInColor = new Color(0.2f, 0.8f, 0.3f, 0.25f);
            var fadeOutColor = new Color(0.8f, 0.3f, 0.2f, 0.25f);
            var fadeInCurveColor = new Color(0.1f, 0.6f, 0.2f, 0.9f);
            var fadeOutCurveColor = new Color(0.6f, 0.2f, 0.1f, 0.9f);

            var transparent = new Color(0, 0, 0, 0);
            var transparentColumn = new Color[height];
            var selectionColumn = new Color[height];

            for (var y = 0; y < height; y++) {
                transparentColumn[y] = transparent;
                selectionColumn[y] = _selectionColor;
            }

            var selectStartX = Mathf.Clamp(Mathf.RoundToInt(selectionStart * width), 0, width - 1);
            var selectEndX = Mathf.Clamp(Mathf.RoundToInt(selectionEnd * width), 0, width);
            var fadeInEndX = fadeInEnd >= 0 ? Mathf.Clamp(Mathf.RoundToInt(fadeInEnd * width), 0, width) : -1;
            var fadeOutStartX = fadeOutStart >= 0 ? Mathf.Clamp(Mathf.RoundToInt(fadeOutStart * width), 0, width) : -1;
            var playheadX = playheadPosition >= 0 ? Mathf.Clamp(Mathf.RoundToInt(playheadPosition * (width - 1)), 0, width - 1) : -1;

            for (var x = 0; x < width; x++) {
                Color[] columnColors;
                var hasSelection = selectionStart < selectionEnd && selectionEnd > selectionStart;
                var inSelection = hasSelection && x >= selectStartX && x < selectEndX;
                var hasFades = fadeInEndX >= 0 || fadeOutStartX >= 0;

                if (inSelection) {
                    columnColors = (Color[])selectionColumn.Clone();

                    if (fadeInEndX > 0 && x >= selectStartX && x < fadeInEndX) {
                        for (var y = 0; y < height; y++) {
                            columnColors[y] = BlendColors(columnColors[y], fadeInColor);
                        }
                    }

                    if (fadeOutStartX >= 0 && x >= fadeOutStartX && x < selectEndX) {
                        for (var y = 0; y < height; y++) {
                            columnColors[y] = BlendColors(columnColors[y], fadeOutColor);
                        }
                    }
                } else if (!hasSelection && hasFades) {
                    columnColors = (Color[])transparentColumn.Clone();

                    if (fadeInEndX > 0 && x < fadeInEndX) {
                        for (var y = 0; y < height; y++) {
                            columnColors[y] = fadeInColor;
                        }
                    }

                    if (fadeOutStartX >= 0 && x >= fadeOutStartX) {
                        for (var y = 0; y < height; y++) {
                            if (fadeInEndX > 0 && x < fadeInEndX) {
                                columnColors[y] = BlendColors(columnColors[y], fadeOutColor);
                            } else {
                                columnColors[y] = fadeOutColor;
                            }
                        }
                    }
                } else {
                    columnColors = transparentColumn;
                }

                texture.SetPixels(x, 0, 1, height, columnColors);
            }

            var effectStartX = (selectionStart < selectionEnd) ? selectStartX : 0;
            var effectEndX = (selectionStart < selectionEnd) ? selectEndX : width;

            if (fadeInCurve != null && fadeInEndX > effectStartX) {
                var fadeWidth = fadeInEndX - effectStartX;
                if (fadeWidth > 5) {
                    var margin = Mathf.Max(2, height / 20);
                    var curveHeight = height - (2 * margin);

                    for (var x = effectStartX; x < fadeInEndX && x < width; x++) {
                        var t = (float)(x - effectStartX) / fadeWidth;
                        var curveValue = fadeInCurve.Evaluate(t);
                        var curveY = margin + Mathf.RoundToInt(curveValue * curveHeight);
                        curveY = Mathf.Clamp(curveY, 0, height - 1);

                        for (var dy = -1; dy <= 1; dy++) {
                            var y = curveY + dy;
                            if (y >= 0 && y < height) {
                                texture.SetPixel(x, y, fadeInCurveColor);
                            }
                        }
                    }
                }
            }

            if (fadeOutCurve != null && fadeOutStartX >= 0 && fadeOutStartX < effectEndX) {
                var fadeWidth = effectEndX - fadeOutStartX;
                if (fadeWidth > 5) {
                    var margin = Mathf.Max(2, height / 20);
                    var curveHeight = height - (2 * margin);

                    for (var x = fadeOutStartX; x < effectEndX && x < width; x++) {
                        var t = (float)(x - fadeOutStartX) / fadeWidth;
                        var curveValue = fadeOutCurve.Evaluate(t);
                        var curveY = margin + Mathf.RoundToInt(curveValue * curveHeight);
                        curveY = Mathf.Clamp(curveY, 0, height - 1);

                        for (var dy = -1; dy <= 1; dy++) {
                            var y = curveY + dy;
                            if (y >= 0 && y < height) {
                                texture.SetPixel(x, y, fadeOutCurveColor);
                            }
                        }
                    }
                }
            }

            if (selectionStart < selectionEnd) {
                var handleColor = new Color(_selectionColor.r, _selectionColor.g, _selectionColor.b, 0.9f);
                var handleColumn = new Color[height];
                for (var y = 0; y < height; y++) {
                    handleColumn[y] = handleColor;
                }

                if (selectStartX >= 0 && selectStartX < width) {
                    texture.SetPixels(selectStartX, 0, 1, height, handleColumn);
                }

                if (selectEndX > 0 && selectEndX <= width) {
                    texture.SetPixels(Mathf.Min(selectEndX - 1, width - 1), 0, 1, height, handleColumn);
                }
            }

            // Draw playhead
            if (playheadX >= 0 && playheadX < width) {
                var playheadColumn = new Color[height];
                for (var y = 0; y < height; y++) {
                    playheadColumn[y] = _playheadColor;
                }

                texture.SetPixels(playheadX, 0, 1, height, playheadColumn);
            }

            texture.Apply();
            return true;
        }

        /// <summary>
        /// Separates interleaved samples into individual channel lists.
        /// </summary>
        private List<List<float>> GetChannelSamples(float[] samples, int channels) {
            var channelSamples = new List<List<float>>();

            for (var i = 0; i < channels; i++) {
                channelSamples.Add(new List<float>());
            }

            for (var i = 0; i < samples.Length; i++) {
                var channel = i % channels;
                channelSamples[channel].Add(samples[i]);
            }

            return channelSamples;
        }

        /// <summary>
        /// Draws a single channel's waveform onto the pixel array.
        /// </summary>
        private void DrawChannel(Color[] pixels, int width, List<float> samples, int minY, int maxY) {
            if (samples == null || samples.Count == 0 || width <= 0 || pixels == null) {
                return;
            }

            var height = maxY - minY;
            if (height <= 0) {
                return;
            }

            var centerY = minY + (height / 2);
            var amplitude = Mathf.Max(1, (height / 2) - 1);
            var totalHeight = pixels.Length / width;

            for (var x = 0; x < width; x++) {
                // Calculate sample range for this pixel column
                var startSampleLong = (long)x * samples.Count / width;
                var endSampleLong = (long)(x + 1) * samples.Count / width;

                var startSample = (int)Mathf.Clamp(startSampleLong, 0, samples.Count - 1);
                var endSample = (int)Mathf.Clamp(endSampleLong, startSample + 1, samples.Count);

                var maxVal = 0f;
                var minVal = 0f;

                // Find min and max in this chunk
                for (var s = startSample; s < endSample && s < samples.Count; s++) {
                    var sample = samples[s];
                    if (sample > maxVal) {
                        maxVal = sample;
                    }

                    if (sample < minVal) {
                        minVal = sample;
                    }
                }

                // Convert to pixel coordinates
                var yTop = centerY + Mathf.RoundToInt(maxVal * amplitude);
                var yBottom = centerY + Mathf.RoundToInt(minVal * amplitude);

                yTop = Mathf.Clamp(yTop, minY, maxY - 1);
                yBottom = Mathf.Clamp(yBottom, minY, maxY - 1);

                // Draw vertical line
                var yStart = Mathf.Min(yTop, yBottom);
                var yEnd = Mathf.Max(yTop, yBottom);

                for (var y = yStart; y <= yEnd; y++) {
                    if (y >= 0 && y < totalHeight && x >= 0 && x < width) {
                        var pixelIndex = (y * width) + x;
                        if (pixelIndex >= 0 && pixelIndex < pixels.Length) {
                            pixels[pixelIndex] = _waveformColor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Blends two colors together using alpha blending.
        /// </summary>
        private Color BlendColors(Color background, Color overlay) {
            var alpha = overlay.a;
            return new Color(
                (background.r * (1 - alpha)) + (overlay.r * alpha),
                (background.g * (1 - alpha)) + (overlay.g * alpha),
                (background.b * (1 - alpha)) + (overlay.b * alpha),
                Mathf.Max(background.a, overlay.a)
            );
        }

        /// <summary>
        /// Converts a normalized position (0-1) to a sample index.
        /// </summary>
        public int NormalizedToSample(float normalized, int totalSamples) {
            return Mathf.RoundToInt(Mathf.Clamp01(normalized) * (totalSamples - 1));
        }

        /// <summary>
        /// Converts a sample index to a normalized position (0-1).
        /// </summary>
        public float SampleToNormalized(int sample, int totalSamples) {
            if (totalSamples <= 0) {
                return 0f;
            }

            return Mathf.Clamp01((float)sample / (totalSamples - 1));
        }

        /// <summary>
        /// Converts a pixel X coordinate to a normalized position.
        /// </summary>
        public float PixelToNormalized(int pixelX, int width) {
            if (width <= 0) {
                return 0f;
            }

            return Mathf.Clamp01((float)pixelX / (width - 1));
        }

        /// <summary>
        /// Converts a normalized position to a pixel X coordinate.
        /// </summary>
        public int NormalizedToPixel(float normalized, int width) {
            return Mathf.RoundToInt(Mathf.Clamp01(normalized) * (width - 1));
        }
    }
}