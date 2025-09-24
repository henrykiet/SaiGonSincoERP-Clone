import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
    selector: '[appArrowNavigation]',
    standalone: true,
})
export class ArrowNavigationDirective {
    constructor(private el: ElementRef<HTMLElement>) { }

    @HostListener('keydown', ['$event'])
    onKeydown(event: KeyboardEvent) {
        if (!event.ctrlKey) return; 
        const active = document.activeElement as HTMLElement | null;
        if (!active || !this.el.nativeElement.contains(active)) return;

        const key = event.key;
        if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(key)) return;

        const focusables = Array.from(
            this.el.nativeElement.querySelectorAll<HTMLElement>(
                'input, select, textarea, [tabindex]:not([tabindex="-1"])'
            )
        ).filter(el => this.isVisible(el));

        const rectActive = active.getBoundingClientRect();

        let candidates: HTMLElement[] = [];
        let closest: HTMLElement | null = null;

        if (key === 'ArrowUp') {
            candidates = focusables.filter(el => el.getBoundingClientRect().top < rectActive.top);
            closest = this.findClosest(candidates, rectActive, 'up');
        } else if (key === 'ArrowDown') {
            candidates = focusables.filter(el => el.getBoundingClientRect().top > rectActive.top);
            closest = this.findClosest(candidates, rectActive, 'down');
        } else if (key === 'ArrowLeft') {
            candidates = focusables.filter(el => el.getBoundingClientRect().left < rectActive.left);
            closest = this.findClosest(candidates, rectActive, 'left');
        } else if (key === 'ArrowRight') {
            candidates = focusables.filter(el => el.getBoundingClientRect().left > rectActive.left);
            closest = this.findClosest(candidates, rectActive, 'right');
        }

        if (closest) {
            closest.focus();
            event.preventDefault();
        }
    }

    private findClosest(candidates: HTMLElement[], rectActive: DOMRect, dir: 'up' | 'down' | 'left' | 'right') {
        let minDistance = Infinity;
        let chosen: HTMLElement | null = null;

        for (const el of candidates) {
            const rect = el.getBoundingClientRect();
            let distance = Infinity;

            if (dir === 'up' || dir === 'down') {
                const dx = Math.abs(rect.left - rectActive.left);
                const dy = Math.abs(rect.top - rectActive.top);
                distance = dx * 10 + dy; // trọng số lớn cho dx
            } else if (dir === 'left' || dir === 'right') {
                const dx = Math.abs(rect.left - rectActive.left);
                const dy = Math.abs(rect.top - rectActive.top);
                distance = dy * 10 + dx; 
            }

            if (distance < minDistance) {
                minDistance = distance;
                chosen = el;
            }
        }
        return chosen;
    }

    private isVisible(el: HTMLElement) {
        return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length);
    }
}
