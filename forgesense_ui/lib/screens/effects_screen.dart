import 'package:flutter/material.dart';
import '../models/effect_item.dart';
import '../services/tcp_service.dart';
import '../theme/app_theme.dart';
import '../widgets/keyboard_preview.dart';

class EffectsScreen extends StatefulWidget {
  final TcpService tcpService;

  const EffectsScreen({super.key, required this.tcpService});

  @override
  State<EffectsScreen> createState() => _EffectsScreenState();
}

class _EffectsScreenState extends State<EffectsScreen> {
  LightEffect _currentEffect = LightEffect.wave;
  String _currentEffectId = 'wave_cyber_neon_spectrum_wave';
  String _currentEffectName = 'Cyber Wave';
  IconData _currentEffectIcon = Icons.waves;
  Color _currentEffectAccent = const Color(0xFF00E5FF);
  double _brightness = 1.0;
  double _speed = 0.7;
  Color _selectedColor = const Color(0xFF00E5FF);
  int _colorVersion = 0;

  String _selectedCategory = 'All (1,000+)';
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  List<EffectItem> get _filteredEffects {
    return EffectsCatalog.allEffects.where((effect) {
      final matchesCategory = _selectedCategory == 'All (1,000+)' ||
          effect.category.toLowerCase() == _selectedCategory.toLowerCase();
      if (!matchesCategory) return false;

      if (_searchQuery.isEmpty) return true;
      final q = _searchQuery.toLowerCase();
      return effect.name.toLowerCase().contains(q) ||
          effect.subcategory.toLowerCase().contains(q) ||
          effect.description.toLowerCase().contains(q) ||
          effect.category.toLowerCase().contains(q);
    }).toList();
  }

  void _selectEffectItem(EffectItem effect) {
    setState(() {
      _currentEffectId = effect.id;
      _currentEffectName = effect.name;
      _currentEffectIcon = effect.icon;
      _currentEffectAccent = effect.accentColor;
      _selectedColor = effect.accentColor;

      // Map base effect to enum
      if (effect.baseEffect.contains('breath')) {
        _currentEffect = LightEffect.breathing;
      } else if (effect.baseEffect.contains('fire')) {
        _currentEffect = LightEffect.fire;
      } else if (effect.baseEffect.contains('matrix')) {
        _currentEffect = LightEffect.matrix;
      } else if (effect.baseEffect.contains('meteor')) {
        _currentEffect = LightEffect.meteor;
      } else if (effect.baseEffect.contains('pulse')) {
        _currentEffect = LightEffect.heartbeat;
      } else if (effect.baseEffect.contains('static')) {
        _currentEffect = LightEffect.static;
      } else if (effect.baseEffect.contains('reactive')) {
        _currentEffect = LightEffect.reactive;
      } else {
        _currentEffect = LightEffect.wave;
      }
    });

    widget.tcpService.sendCommand(effect.command);
    widget.tcpService.sendCommand(
      'COLOR:${effect.accentColor.red},${effect.accentColor.green},${effect.accentColor.blue}:${++_colorVersion}',
    );
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final displayedList = _filteredEffects;

    return Padding(
      padding: const EdgeInsets.all(24.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Studio Header & Stats
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('RGB Studio', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontSize: 28, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text('Master library of 1,000+ gaming, anime, wave, and ambient light choreographies.',
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: AppTheme.onSurfaceVariant)),
                ],
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                decoration: BoxDecoration(
                  color: AppTheme.surfaceContainerHigh,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: AppTheme.primary.withOpacity(0.4)),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.auto_awesome, color: AppTheme.primary, size: 16),
                    const SizedBox(width: 8),
                    Text(
                      '${EffectsCatalog.allEffects.length} Effects Loaded',
                      style: const TextStyle(color: AppTheme.primary, fontWeight: FontWeight.bold, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Interactive Keyboard Preview & Quick Sliders Banner
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppTheme.surfaceContainerHigh.withOpacity(0.85),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.white.withOpacity(0.1)),
            ),
            child: Row(
              children: [
                // Keyboard preview widget
                Expanded(
                  flex: 3,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Row(
                            children: [
                              Icon(_currentEffectIcon, color: _currentEffectAccent, size: 18),
                              const SizedBox(width: 8),
                              Text('Active Live Simulation: ', style: const TextStyle(color: Colors.white70, fontSize: 12)),
                              Text(
                                _currentEffectName,
                                style: TextStyle(color: _currentEffectAccent, fontWeight: FontWeight.bold, fontSize: 13),
                              ),
                            ],
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: Colors.white10,
                              borderRadius: BorderRadius.circular(4),
                            ),
                            child: const Text('Live Sync On', style: TextStyle(color: Color(0xFF00E676), fontSize: 11, fontWeight: FontWeight.w600)),
                          ),
                        ],
                      ),
                      const SizedBox(height: 10),
                      Container(
                        height: 160,
                        decoration: BoxDecoration(
                          color: AppTheme.surface,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: AppTheme.surfaceContainerHigh),
                        ),
                        child: KeyboardPreview(
                          effect: _currentEffect,
                          effectId: _currentEffectId,
                          baseColor: _selectedColor,
                          brightness: _brightness,
                          speed: _speed,
                          onZoneColorsChanged: (z0, z1, z2, z3) {
                            widget.tcpService.streamZoneColors(z0, z1, z2, z3);
                          },
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 20),
                // Quick Parameters Column
                Expanded(
                  flex: 1,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('QUICK TUNING', style: TextStyle(color: AppTheme.primary, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 1.1)),
                      const SizedBox(height: 12),
                      _buildMiniSlider(Icons.light_mode, 'Brightness', _brightness, (val) {
                        setState(() { _brightness = val; });
                        widget.tcpService.sendCommand('BRIGHTNESS:${val.toStringAsFixed(2)}');
                      }),
                      const SizedBox(height: 12),
                      _buildMiniSlider(Icons.speed, 'Speed', _speed, (val) {
                        setState(() { _speed = val; });
                        widget.tcpService.sendCommand('SPEED:${val.toStringAsFixed(2)}');
                      }),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // Search Bar & Stats
          Row(
            children: [
              Expanded(
                child: Container(
                  height: 42,
                  decoration: BoxDecoration(
                    color: AppTheme.surfaceContainerHigh,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.white.withOpacity(0.1)),
                  ),
                  child: TextField(
                    controller: _searchController,
                    onChanged: (val) {
                      setState(() {
                        _searchQuery = val;
                      });
                    },
                    style: const TextStyle(fontSize: 13, color: Colors.white),
                    decoration: InputDecoration(
                      hintText: 'Search 1,000+ effects (e.g. Valorant, Neon, Matrix, Cyber, Sun, Ice)...',
                      hintStyle: TextStyle(color: Colors.white.withOpacity(0.4), fontSize: 13),
                      prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.primary),
                      suffixIcon: _searchQuery.isNotEmpty
                          ? IconButton(
                              icon: const Icon(Icons.close, size: 16, color: Colors.white54),
                              onPressed: () {
                                _searchController.clear();
                                setState(() {
                                  _searchQuery = '';
                                });
                              },
                            )
                          : null,
                      border: InputBorder.none,
                      contentPadding: const EdgeInsets.symmetric(vertical: 11),
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                decoration: BoxDecoration(
                  color: AppTheme.surfaceContainerHigh,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.white.withOpacity(0.1)),
                ),
                child: Text(
                  'Showing ${displayedList.length} of ${EffectsCatalog.allEffects.length}',
                  style: const TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),

          // Category Chips
          SizedBox(
            height: 36,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              itemCount: EffectsCatalog.categories.length,
              separatorBuilder: (_, _) => const SizedBox(width: 8),
              itemBuilder: (context, idx) {
                final cat = EffectsCatalog.categories[idx];
                final isSelected = _selectedCategory == cat;
                return ChoiceChip(
                  label: Text(cat),
                  selected: isSelected,
                  onSelected: (_) {
                    setState(() {
                      _selectedCategory = cat;
                    });
                  },
                  labelStyle: TextStyle(
                    fontSize: 12,
                    fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                    color: isSelected ? Colors.black : Colors.white70,
                  ),
                  selectedColor: AppTheme.primary,
                  backgroundColor: AppTheme.surfaceContainerHigh,
                  side: BorderSide(
                    color: isSelected ? AppTheme.primary : Colors.white.withOpacity(0.1),
                  ),
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 0),
                );
              },
            ),
          ),
          const SizedBox(height: 14),

          // Grid of 1,000+ Effects
          Expanded(
            child: displayedList.isEmpty
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.search_off, size: 48, color: Colors.white24),
                        const SizedBox(height: 8),
                        Text('No effects found matching "$_searchQuery"',
                            style: const TextStyle(color: Colors.white54, fontSize: 14)),
                      ],
                    ),
                  )
                : GridView.builder(
                    gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 5,
                      crossAxisSpacing: 12,
                      mainAxisSpacing: 12,
                      childAspectRatio: 1.8,
                    ),
                    itemCount: displayedList.length,
                    itemBuilder: (context, index) {
                      final effect = displayedList[index];
                      final isSelected = _currentEffectId == effect.id;

                      return InkWell(
                        onTap: () => _selectEffectItem(effect),
                        borderRadius: BorderRadius.circular(10),
                        child: AnimatedContainer(
                          duration: const Duration(milliseconds: 150),
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: isSelected
                                ? effect.accentColor.withOpacity(0.18)
                                : AppTheme.surfaceContainerHigh.withOpacity(0.85),
                            borderRadius: BorderRadius.circular(10),
                            border: Border.all(
                              color: isSelected
                                  ? effect.accentColor
                                  : Colors.white.withOpacity(0.08),
                              width: isSelected ? 2.0 : 1.0,
                            ),
                            boxShadow: isSelected
                                ? [
                                    BoxShadow(
                                      color: effect.accentColor.withOpacity(0.35),
                                      blurRadius: 10,
                                      spreadRadius: 1,
                                    ),
                                  ]
                                : null,
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Container(
                                    width: 32,
                                    height: 32,
                                    decoration: BoxDecoration(
                                      color: effect.accentColor.withOpacity(0.2),
                                      borderRadius: BorderRadius.circular(6),
                                      border: Border.all(color: effect.accentColor.withOpacity(0.5)),
                                    ),
                                    child: Icon(effect.icon, color: effect.accentColor, size: 18),
                                  ),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                    decoration: BoxDecoration(
                                      color: Colors.white10,
                                      borderRadius: BorderRadius.circular(4),
                                    ),
                                    child: Text(
                                      effect.subcategory,
                                      style: TextStyle(color: effect.accentColor, fontSize: 10, fontWeight: FontWeight.bold),
                                    ),
                                  ),
                                ],
                              ),
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    effect.name,
                                    style: TextStyle(
                                      color: isSelected ? Colors.white : Colors.white.withOpacity(0.95),
                                      fontWeight: isSelected ? FontWeight.bold : FontWeight.w600,
                                      fontSize: 13,
                                    ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    effect.description,
                                    style: TextStyle(
                                      color: Colors.white.withOpacity(0.5),
                                      fontSize: 10,
                                    ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildMiniSlider(IconData icon, String label, double value, ValueChanged<double> onChanged) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              children: [
                Icon(icon, color: AppTheme.onSurfaceVariant, size: 14),
                const SizedBox(width: 4),
                Text(label, style: const TextStyle(fontSize: 11, color: Colors.white70)),
              ],
            ),
            Text('${(value * 100).toInt()}%', style: const TextStyle(fontSize: 11, color: AppTheme.primary, fontWeight: FontWeight.bold)),
          ],
        ),
        SliderTheme(
          data: SliderTheme.of(context).copyWith(
            trackHeight: 3,
            thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 5),
          ),
          child: Slider(
            value: value,
            onChanged: onChanged,
            activeColor: AppTheme.primary,
            inactiveColor: AppTheme.surfaceContainer,
          ),
        ),
      ],
    );
  }
}
