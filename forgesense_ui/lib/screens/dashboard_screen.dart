import 'package:flutter/material.dart';
import '../models/effect_item.dart';
import '../services/tcp_service.dart';
import '../theme/app_theme.dart';
import '../widgets/keyboard_preview.dart';

class DashboardScreen extends StatefulWidget {
  final TcpService tcpService;
  final VoidCallback? onOpenRgbStudio;

  const DashboardScreen({super.key, required this.tcpService, this.onOpenRgbStudio});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
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

  void _setColor(Color c) {
    setState(() {
      _selectedColor = c;
      _colorVersion++;
      if (_currentEffect != LightEffect.breathing &&
          _currentEffect != LightEffect.heartbeat &&
          _currentEffect != LightEffect.reactive) {
        _currentEffect = LightEffect.static;
        _currentEffectId = 'static';
        _currentEffectName = 'Static Custom Color';
      }
    });
    widget.tcpService.sendCommand('COLOR:${c.red},${c.green},${c.blue}:$_colorVersion');
    if (_currentEffect == LightEffect.static) {
      widget.tcpService.sendCommand('MODE:STATIC');
    }
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
    return ListenableBuilder(
      listenable: widget.tcpService,
      builder: (context, _) {
        final isAcerRunning = widget.tcpService.acerServiceStatus == 'RUNNING';
        final isAcerStopped = widget.tcpService.acerServiceStatus == 'STOPPED';
        final displayedList = _filteredEffects;

        return Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Good afternoon.', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontSize: 28, fontWeight: FontWeight.bold)),
                      const SizedBox(height: 4),
                      Text('Customize keyboard lighting across 1,000+ effects.',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: AppTheme.onSurfaceVariant)),
                    ],
                  ),
                  if (widget.onOpenRgbStudio != null)
                    OutlinedButton.icon(
                      onPressed: widget.onOpenRgbStudio,
                      icon: const Icon(Icons.palette_outlined, size: 16),
                      label: const Text('Open RGB Studio'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppTheme.primary,
                        side: const BorderSide(color: AppTheme.primary),
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                      ),
                    ),
                ],
              ),
              const SizedBox(height: 16),

              // Service Status Banner
              if (isAcerRunning)
                Container(
                  margin: const EdgeInsets.only(bottom: 16),
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFFA726).withOpacity(0.12),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: const Color(0xFFFFA726).withOpacity(0.4)),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.warning_amber_rounded, color: Color(0xFFFFA726), size: 22),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Acer Lighting Service is Running',
                              style: TextStyle(fontWeight: FontWeight.bold, color: Color(0xFFFFCC80), fontSize: 13),
                            ),
                            Text(
                              'OEM service may override custom lighting effects. Stop it to enable full PredatorRGB control.',
                              style: TextStyle(color: Colors.white.withOpacity(0.7), fontSize: 11),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(width: 12),
                      ElevatedButton.icon(
                        onPressed: widget.tcpService.isProcessingService
                            ? null
                            : () => widget.tcpService.stopAcerLightingService(),
                        icon: widget.tcpService.isProcessingService
                            ? const SizedBox(
                                width: 14,
                                height: 14,
                                child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                              )
                            : const Icon(Icons.stop_circle_outlined, size: 16),
                        label: const Text('Stop Service'),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFFD32F2F),
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                          textStyle: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                        ),
                      ),
                    ],
                  ),
                )
              else if (isAcerStopped)
                Container(
                  margin: const EdgeInsets.only(bottom: 16),
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                  decoration: BoxDecoration(
                    color: const Color(0xFF00E676).withOpacity(0.08),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: const Color(0xFF00E676).withOpacity(0.25)),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.check_circle_outline, color: Color(0xFF00E676), size: 16),
                      const SizedBox(width: 8),
                      const Text(
                        'Acer Lighting Service is Stopped — PredatorRGB has exclusive hardware control.',
                        style: TextStyle(color: Color(0xFFB9F6CA), fontSize: 12),
                      ),
                      const Spacer(),
                      TextButton(
                        onPressed: () => widget.tcpService.startAcerLightingService(),
                        child: const Text('Restore Service', style: TextStyle(color: AppTheme.onSurfaceVariant, fontSize: 11)),
                      ),
                    ],
                  ),
                ),

              // Content Area
              Expanded(
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Main Area: Keyboard Visualization + 1,000+ Effects Explorer
                    Expanded(
                      flex: 5,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Keyboard Visualizer Card
                          Container(
                            padding: const EdgeInsets.all(20),
                            decoration: BoxDecoration(
                              color: AppTheme.surfaceContainerHigh.withOpacity(0.85),
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: Colors.white.withOpacity(0.1)),
                            ),
                            child: Column(
                              children: [
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    Row(
                                      children: [
                                        Icon(_currentEffectIcon, color: _currentEffectAccent, size: 20),
                                        const SizedBox(width: 8),
                                        Text('4-Zone Keyboard Live Preview',
                                            style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
                                      ],
                                    ),
                                    Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                      decoration: BoxDecoration(
                                        color: AppTheme.surfaceContainer,
                                        borderRadius: BorderRadius.circular(4),
                                        border: Border.all(color: _currentEffectAccent.withOpacity(0.4)),
                                      ),
                                      child: Text(
                                        _currentEffectName,
                                        style: TextStyle(color: _currentEffectAccent, fontSize: 12, fontWeight: FontWeight.bold),
                                      ),
                                    ),
                                  ],
                                ),
                                const Divider(color: Colors.white10, height: 20),
                                // Dynamic Keyboard Preview
                                Container(
                                  height: 200,
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
                          const SizedBox(height: 16),

                          // Search and Category Tabs
                          Row(
                            children: [
                              // Search Box
                              Expanded(
                                flex: 2,
                                child: Container(
                                  height: 40,
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
                                      hintText: 'Search 1,000+ effects (e.g. Valorant, Neon, Wave)...',
                                      hintStyle: TextStyle(color: Colors.white.withOpacity(0.4), fontSize: 12),
                                      prefixIcon: const Icon(Icons.search, size: 18, color: AppTheme.primary),
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
                                      contentPadding: const EdgeInsets.symmetric(vertical: 10),
                                    ),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 12),
                              // Counter Badge
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                                decoration: BoxDecoration(
                                  color: AppTheme.surfaceContainerHigh,
                                  borderRadius: BorderRadius.circular(8),
                                  border: Border.all(color: Colors.white.withOpacity(0.1)),
                                ),
                                child: Text(
                                  '${displayedList.length} of ${EffectsCatalog.allEffects.length} Effects',
                                  style: const TextStyle(color: AppTheme.primary, fontSize: 12, fontWeight: FontWeight.bold),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 10),

                          // Category Filter Pills
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
                                    fontSize: 11,
                                    fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                                    color: isSelected ? Colors.black : Colors.white70,
                                  ),
                                  selectedColor: AppTheme.primary,
                                  backgroundColor: AppTheme.surfaceContainerHigh,
                                  side: BorderSide(
                                    color: isSelected ? AppTheme.primary : Colors.white.withOpacity(0.1),
                                  ),
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 0),
                                );
                              },
                            ),
                          ),
                          const SizedBox(height: 12),

                          // Effects Grid (Browse all 1,000+ effects with instant preview)
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
                                      crossAxisCount: 4,
                                      crossAxisSpacing: 10,
                                      mainAxisSpacing: 10,
                                      childAspectRatio: 2.1,
                                    ),
                                    itemCount: displayedList.length,
                                    itemBuilder: (context, index) {
                                      final effect = displayedList[index];
                                      final isSelected = _currentEffectId == effect.id;

                                      return InkWell(
                                        onTap: () => _selectEffectItem(effect),
                                        borderRadius: BorderRadius.circular(8),
                                        child: AnimatedContainer(
                                          duration: const Duration(milliseconds: 150),
                                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
                                          decoration: BoxDecoration(
                                            color: isSelected
                                                ? effect.accentColor.withOpacity(0.18)
                                                : AppTheme.surfaceContainerHigh.withOpacity(0.8),
                                            borderRadius: BorderRadius.circular(8),
                                            border: Border.all(
                                              color: isSelected
                                                  ? effect.accentColor
                                                  : Colors.white.withOpacity(0.08),
                                              width: isSelected ? 1.8 : 1.0,
                                            ),
                                            boxShadow: isSelected
                                                ? [
                                                    BoxShadow(
                                                      color: effect.accentColor.withOpacity(0.3),
                                                      blurRadius: 8,
                                                      spreadRadius: 1,
                                                    ),
                                                  ]
                                                : null,
                                          ),
                                          child: Row(
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
                                              const SizedBox(width: 8),
                                              Expanded(
                                                child: Column(
                                                  crossAxisAlignment: CrossAxisAlignment.start,
                                                  mainAxisAlignment: MainAxisAlignment.center,
                                                  children: [
                                                    Text(
                                                      effect.name,
                                                      style: TextStyle(
                                                        color: isSelected ? Colors.white : Colors.white.withOpacity(0.9),
                                                        fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
                                                        fontSize: 12,
                                                      ),
                                                      maxLines: 1,
                                                      overflow: TextOverflow.ellipsis,
                                                    ),
                                                    const SizedBox(height: 2),
                                                    Row(
                                                      children: [
                                                        Container(
                                                          padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 1),
                                                          decoration: BoxDecoration(
                                                            color: Colors.white10,
                                                            borderRadius: BorderRadius.circular(3),
                                                          ),
                                                          child: Text(
                                                            effect.subcategory,
                                                            style: TextStyle(color: effect.accentColor, fontSize: 9, fontWeight: FontWeight.bold),
                                                          ),
                                                        ),
                                                      ],
                                                    ),
                                                  ],
                                                ),
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
                    ),
                    const SizedBox(width: 16),

                    // Sidebar: Parameters & Profile Info
                    Expanded(
                      flex: 2,
                      child: SingleChildScrollView(
                        child: Column(
                          children: [
                            // Active Configuration Card
                            Container(
                              padding: const EdgeInsets.all(18),
                              decoration: BoxDecoration(
                                color: AppTheme.surfaceContainerHigh.withOpacity(0.85),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: Colors.white.withOpacity(0.1)),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text('ACTIVE CONFIGURATION', style: Theme.of(context).textTheme.labelSmall),
                                  const Divider(color: Colors.white10, height: 20),
                                  Text('Active Effect', style: Theme.of(context).textTheme.bodySmall?.copyWith(color: AppTheme.onSurfaceVariant)),
                                  const SizedBox(height: 4),
                                  Row(
                                    children: [
                                      Icon(_currentEffectIcon, color: _currentEffectAccent, size: 18),
                                      const SizedBox(width: 6),
                                      Expanded(
                                        child: Text(
                                          _currentEffectName,
                                          style: TextStyle(color: _currentEffectAccent, fontWeight: FontWeight.bold, fontSize: 14),
                                          maxLines: 1,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 12),
                                  Text('Base Mode', style: Theme.of(context).textTheme.bodySmall?.copyWith(color: AppTheme.onSurfaceVariant)),
                                  const SizedBox(height: 4),
                                  Row(
                                    children: [
                                      const Icon(Icons.flash_on, color: AppTheme.primary, size: 16),
                                      const SizedBox(width: 6),
                                      Text(
                                        _currentEffect.name.toUpperCase(),
                                        style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600),
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 16),

                            // Parameters Card
                            Container(
                              padding: const EdgeInsets.all(18),
                              decoration: BoxDecoration(
                                color: AppTheme.surfaceContainerHigh.withOpacity(0.85),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: Colors.white.withOpacity(0.1)),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text('HARDWARE PARAMETERS', style: Theme.of(context).textTheme.labelSmall),
                                  const Divider(color: Colors.white10, height: 20),
                                  _buildSlider(context, Icons.light_mode, 'Brightness', _brightness, (val) {
                                    setState(() { _brightness = val; });
                                    widget.tcpService.sendCommand('BRIGHTNESS:${val.toStringAsFixed(2)}');
                                  }),
                                  const SizedBox(height: 12),
                                  _buildSlider(context, Icons.speed, 'Animation Speed', _speed, (val) {
                                    setState(() { _speed = val; });
                                    widget.tcpService.sendCommand('SPEED:${val.toStringAsFixed(2)}');
                                  }),
                                  const SizedBox(height: 14),
                                  Text('QUICK COLOR PALETTE', style: Theme.of(context).textTheme.labelSmall),
                                  const Divider(color: Colors.white10, height: 16),
                                  Wrap(
                                    spacing: 8,
                                    runSpacing: 8,
                                    children: [
                                      const Color(0xFF00E5FF),
                                      const Color(0xFFFF007F),
                                      const Color(0xFF7C4DFF),
                                      const Color(0xFF00FF66),
                                      const Color(0xFFFFD700),
                                      const Color(0xFFFF6D00),
                                      const Color(0xFFFF1744),
                                      const Color(0xFF80D8FF),
                                      const Color(0xFF0091EA),
                                      const Color(0xFFAB47BC),
                                      const Color(0xFF00E676),
                                      const Color(0xFFECEFF1),
                                    ].map((c) => InkWell(
                                      onTap: () => _setColor(c),
                                      child: Container(
                                        width: 28,
                                        height: 28,
                                        decoration: BoxDecoration(
                                          color: c,
                                          shape: BoxShape.circle,
                                          border: Border.all(
                                            color: _selectedColor == c ? Colors.white : Colors.transparent,
                                            width: 2,
                                          ),
                                          boxShadow: [
                                            BoxShadow(
                                              color: c.withOpacity(0.4),
                                              blurRadius: 4,
                                              spreadRadius: 1,
                                            ),
                                          ],
                                        ),
                                      ),
                                    )).toList(),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 16),

                            // Reset to Default Wave
                            SizedBox(
                              width: double.infinity,
                              height: 44,
                              child: OutlinedButton.icon(
                                onPressed: () {
                                  final def = EffectsCatalog.allEffects.first;
                                  _selectEffectItem(def);
                                },
                                icon: const Icon(Icons.restore, size: 16),
                                label: const Text('Reset to Default Wave'),
                                style: OutlinedButton.styleFrom(
                                  foregroundColor: AppTheme.onSurfaceVariant,
                                  side: BorderSide(color: Colors.white.withOpacity(0.15)),
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildSlider(BuildContext context, IconData icon, String label, double value, ValueChanged<double> onChanged) {
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              children: [
                Icon(icon, color: AppTheme.onSurfaceVariant, size: 15),
                const SizedBox(width: 4),
                Text(label, style: const TextStyle(fontSize: 12, color: Colors.white70)),
              ],
            ),
            Text('${(value * 100).toInt()}%', style: const TextStyle(fontSize: 12, color: AppTheme.primary, fontWeight: FontWeight.bold)),
          ],
        ),
        SliderTheme(
          data: SliderTheme.of(context).copyWith(
            trackHeight: 3,
            thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 6),
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
