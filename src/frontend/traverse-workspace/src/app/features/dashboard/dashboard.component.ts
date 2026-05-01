// src/app/features/dashboard/dashboard.component.ts
//
// DashboardComponent — Phase 0 placeholder dashboard with 6 KPI tile slots.
//
// Phase 0 stub — KPI slot content replaced by MOD-01/MOD-03 in Phase 1.
//
// WHY GENERIC SLOT NAMES: The actual KPI metric names are defined by MOD-03
// (Phase 1b). Using "KPI Slot 1–6" prevents the design from being constrained
// by assumed metric names that may change. The 3-column grid layout establishes
// the visual contract that Phase 1 will populate. (Decision DES-002.)
//
// PRESENTATIONAL: This component has no service injection and no HTTP calls.
// It uses a static kpiSlots array. Phase 1 replaces this with a smart container
// that injects the KPI store and renders real metric data.
//
// AC-031: 6 KPI placeholder tiles with status='slate'
// AC-028: standalone, OnPush

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { KpiStatusBadgeComponent } from '@shared/components/kpi-status-badge/kpi-status-badge.component';

/**
 * Slot definition for a Phase 0 KPI placeholder tile.
 *
 * All slots use 'slate' status to indicate placeholder content.
 * The label provides a position reference ("KPI Slot 1–6") for layout verification.
 */
interface KpiSlot {
  label: string;
  subtitle: string;
}

/**
 * Dashboard placeholder — renders 6 KPI slot tiles in a 3-column grid.
 *
 * Phase 0 stub: all tiles show 'slate' status with generic labels.
 * Replaced by real KPI data in MOD-01/MOD-03 Phase 1.
 */
// Phase 0 stub — KPI slot content replaced in Phase 1 (MOD-01/MOD-03)
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [PageHeaderComponent, KpiStatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Dashboard"
      subtitle="Phase 0 — KPI metrics available after Phase 1 implementation"
    />

    <div class="kpi-grid">
      @for (slot of kpiSlots; track slot.label) {
        <div class="kpi-tile">
          <app-kpi-status-badge status="slate" [label]="slot.label" />
          <p class="kpi-tile__subtitle">{{ slot.subtitle }}</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-top: 24px;
    }

    .kpi-tile {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding: 16px;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 8px;
      background-color: var(--mat-sys-surface-container);
    }

    .kpi-tile__subtitle {
      font-size: 12px;
      color: var(--mat-sys-on-surface-variant);
      margin: 0;
    }

    /* Responsive: 2 columns on tablet, 1 on mobile */
    @media (max-width: 1024px) {
      .kpi-grid { grid-template-columns: repeat(2, 1fr); }
    }
    @media (max-width: 767px) {
      .kpi-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class DashboardComponent {
  /**
   * Static Phase 0 KPI slot definitions.
   *
   * Generic labels are intentional — actual KPI metric names are defined by
   * MOD-03 in Phase 1. Using specific names now would create incorrect assumptions.
   * (Decision DES-002 in story tracker.)
   */
  // Phase 0 stub — replace with real KPI store data in Phase 1 (MOD-01/MOD-03)
  protected readonly kpiSlots: readonly KpiSlot[] = [
    { label: 'KPI Slot 1', subtitle: 'Available in Phase 1' },
    { label: 'KPI Slot 2', subtitle: 'Available in Phase 1' },
    { label: 'KPI Slot 3', subtitle: 'Available in Phase 1' },
    { label: 'KPI Slot 4', subtitle: 'Available in Phase 1' },
    { label: 'KPI Slot 5', subtitle: 'Available in Phase 1' },
    { label: 'KPI Slot 6', subtitle: 'Available in Phase 1' },
  ];
}
