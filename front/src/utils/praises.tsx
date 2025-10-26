// Configuration constants for the floating quotes
export const QUOTE_CONFIG = {
  // Quote selection
  MIN_QUOTES: 12,           // Minimum number of quotes to display
  MAX_QUOTES: 16,          // Maximum number of quotes to display
  
  // Positioning
  LEFT_SIDE_MIN: 15,        // Minimum position for left side quotes (%)
  LEFT_SIDE_MAX: 35,       // Maximum position for left side quotes (%)
  RIGHT_SIDE_MIN: 65,      // Minimum position for right side quotes (%)
  RIGHT_SIDE_MAX: 85,      // Maximum position for right side quotes (%)
  TOP_MIN: 5,              // Minimum vertical position (%)
  TOP_MAX: 95,             // Maximum vertical position (%)
  VERTICAL_SEGMENTS: 5,    // Number of vertical segments for better distribution
  
  // Movement
  MOVEMENT_ENABLED: true,  // Enable multi-directional movement for dots
  MOVEMENT_RANGE_X: 1.5,   // Horizontal movement range (px)
  MOVEMENT_RANGE_Y: 1.5,   // Vertical movement range (px)
  MOVEMENT_DURATION: 5,    // Movement animation duration (seconds)
  MOVEMENT_RANDOM: true,   // Use random movement pattern
  
  // Sizing
  MIN_FONT_SIZE: 0.8,      // Minimum font size (rem)
  MAX_FONT_SIZE: 1.8,      // Maximum font size (rem)
  
  // Appearance
  MIN_OPACITY: 0.50,       // Minimum opacity
  MAX_OPACITY: 0.75,       // Maximum opacity
  TEXT_COLOR: 'rgba(100,116,139,0.6)', // Text color
  
  // dot configuration
  DOT_COLOR: 'rgba(255, 191, 0, 1)',   // Base color for the dot
  DOT_GLOW_COLOR: 'rgba(255, 215, 0, 0.6)', // Glow color for the dot
  DOT_SIZE: '1.4em',       // Size of the dot relative to text
  DOT_PULSE_DURATION: '3s', // Duration of the pulse animation
  DOT_PULSE_MIN: 0.3,      // Minimum opacity during pulse
  DOT_PULSE_MAX: 1,        // Maximum opacity during pulse
  DOT_CHAR: '.',           // Character to use for the dot
  
  LETTER_SPACING: '-0.02em', // Letter spacing
  ROTATION: '0deg',          // Rotation (set to 0 for upright quotes)
  
  // Animation
  MIN_DURATION: 90,        // Minimum animation duration (seconds)
  MAX_DURATION: 90,       // Maximum animation duration (seconds)
  MAX_DELAY: 60,           // Maximum animation delay (seconds)
  
  // Overlap prevention
  WIDTH_FACTOR: 0.5,       // Factor to estimate width based on text length and font size
  MIN_HORIZONTAL_SPACING: 0.6, // Minimum horizontal spacing between quotes (multiplier of combined widths)
  MIN_VERTICAL_SPACING: 15.0,   // Minimum vertical spacing between quotes (multiplier of combined heights)
  MAX_PLACEMENT_ATTEMPTS: 30,  // Maximum attempts to find non-overlapping position
};

// Full list of praises
export const ALL_PRAISES = [
  "Eigintas: einu ginti",
  "Revolutionary music sharing platform",
  "Connect with friends through sound",
  "The future of collaborative listening",
  "Where music meets community",
  "Seamless song queue management",
  "Real-time chat integration",
  "Discover new music together",
  "Built with cutting-edge technology",
  "Intuitive user experience",
  "Team-based music exploration",
  "YouTube integration done right",
  "Next-generation playlist sharing",
  "Connect through music",
  "Share the rhythm, share the moment",
  "Music that brings people together",
  "The social music experience",
  "Queue up the good vibes",
  "Your soundtrack, our platform",
  "Collaboration meets melody",
  "Modern music, modern connections",
];

// Types for quote styles
export interface QuotePosition {
  left: number;
  top: number;
  fontSize: number;
  opacity: number;
  animationDuration: number;
}

export interface QuoteStyle {
  left: string;
  top: string;
  fontSize: string;
  opacity: number;
  animationDuration: string;
  animationDelay: string;
  color: string;
  letterSpacing: string;
  rotation: string;
}

// Get a random subset of praises
export const getRandomPraises = () => {
  const shuffled = [...ALL_PRAISES].sort(() => 0.5 - Math.random());
  // Random number between MIN_QUOTES and MAX_QUOTES for more natural variation
  const count = Math.floor(Math.random() * (QUOTE_CONFIG.MAX_QUOTES - QUOTE_CONFIG.MIN_QUOTES + 1)) + QUOTE_CONFIG.MIN_QUOTES;
  return shuffled.slice(0, count);
};

// Generate random positions and sizes like scattered leaves
export const generatePraiseStyles = (praises: string[]): QuoteStyle[] => {
  // Distribute quotes across the entire width
  const positions: QuotePosition[] = [];
  
  // Create a more natural distribution of quotes
  return praises.map((praise) => {
    // Generate random position across the entire width
    let leftPosition: number = 0;
    let topPosition: number = 0;
    let fontSize: number = 0;
    let attempts = 0;
    
    // Keep trying positions until we find one that doesn't overlap
    do {
      // Position quotes more towards the sides (avoid the center)
      // 50% chance of being on the left side, 50% chance of being on the right side
      if (Math.random() > 0.5) {
        leftPosition = Math.random() * (QUOTE_CONFIG.LEFT_SIDE_MAX - QUOTE_CONFIG.LEFT_SIDE_MIN) + QUOTE_CONFIG.LEFT_SIDE_MIN;
      } else {
        leftPosition = Math.random() * (QUOTE_CONFIG.RIGHT_SIDE_MAX - QUOTE_CONFIG.RIGHT_SIDE_MIN) + QUOTE_CONFIG.RIGHT_SIDE_MIN;
      }
      
      // Better vertical distribution using segments
      // Determine which vertical segment this quote belongs to
      const verticalSegment = Math.floor(Math.random() * QUOTE_CONFIG.VERTICAL_SEGMENTS);
      const segmentHeight = (QUOTE_CONFIG.TOP_MAX - QUOTE_CONFIG.TOP_MIN) / QUOTE_CONFIG.VERTICAL_SEGMENTS;
      
      // Calculate position within the segment (with some randomness)
      const segmentTop = QUOTE_CONFIG.TOP_MIN + (verticalSegment * segmentHeight);
      topPosition = segmentTop + (Math.random() * segmentHeight);
      
      // Smaller font sizes to ensure quotes stay within screen
      fontSize = Math.random() * (QUOTE_CONFIG.MAX_FONT_SIZE - QUOTE_CONFIG.MIN_FONT_SIZE) + QUOTE_CONFIG.MIN_FONT_SIZE;
      
      attempts++;
    } while (
      attempts < QUOTE_CONFIG.MAX_PLACEMENT_ATTEMPTS && 
      positions.some(pos => {
        // Calculate approximate width of this quote based on font size and text length
        const thisWidth = praise.length * fontSize * QUOTE_CONFIG.WIDTH_FACTOR;
        const otherWidth = praises[positions.indexOf(pos)].length * pos.fontSize * QUOTE_CONFIG.WIDTH_FACTOR;
        
        // Calculate distance between quotes
        const xDist = Math.abs(pos.left - leftPosition);
        const yDist = Math.abs(pos.top - topPosition);
        
        // Ensure quotes are far enough apart based on their sizes
        return (xDist < (thisWidth + otherWidth) * QUOTE_CONFIG.MIN_HORIZONTAL_SPACING) && 
               (yDist < (pos.fontSize + fontSize) * QUOTE_CONFIG.MIN_VERTICAL_SPACING);
      })
    );
    
    // Increase opacity to make quotes more visible
    const opacity = Math.random() * (QUOTE_CONFIG.MAX_OPACITY - QUOTE_CONFIG.MIN_OPACITY) + QUOTE_CONFIG.MIN_OPACITY;
    
    // Vary animation duration for more natural movement
    const animationDuration = Math.random() * (QUOTE_CONFIG.MAX_DURATION - QUOTE_CONFIG.MIN_DURATION) + QUOTE_CONFIG.MIN_DURATION;
    
    // Store this position to check for future overlaps
    const position = {
      left: leftPosition,
      top: topPosition,
      fontSize,
      opacity,
      animationDuration
    };
    positions.push(position);
    
    return {
      left: `${leftPosition}%`,
      top: `${topPosition}%`,
      fontSize: `${fontSize}rem`,
      opacity,
      animationDuration: `${position.animationDuration}s`,
      animationDelay: `${-(Math.random() * QUOTE_CONFIG.MAX_DELAY)}s`, // Stagger the start times
      color: QUOTE_CONFIG.TEXT_COLOR,
      letterSpacing: QUOTE_CONFIG.LETTER_SPACING,
      rotation: QUOTE_CONFIG.ROTATION,
    };
  });
};

// CSS for floating quotes animation
export const floatingQuotesCSS = `
  @keyframes float-up {
    0% {
      transform: translateY(100vh) translateX(-50%) rotate(var(--rotation));
      opacity: 0;
    }
    10% {
      opacity: var(--opacity);
    }
    90% {
      opacity: var(--opacity);
    }
    100% {
      transform: translateY(-50vh) translateX(-50%) rotate(var(--rotation));
      opacity: 0;
    }
  }
  
  @keyframes star-pulse {
    0%, 100% {
      opacity: ${QUOTE_CONFIG.DOT_PULSE_MIN};
      transform: scale(0.9);
      text-shadow: 0 0 5px ${QUOTE_CONFIG.DOT_GLOW_COLOR};
    }
    50% {
      opacity: ${QUOTE_CONFIG.DOT_PULSE_MAX};
      transform: scale(1.1);
      text-shadow: 0 0 15px ${QUOTE_CONFIG.DOT_GLOW_COLOR}, 0 0 20px ${QUOTE_CONFIG.DOT_GLOW_COLOR};
    }
  }
  
  @keyframes float-around {
    0%, 100% {
      transform: translate(0, 0);
    }
    20% {
      transform: translate(${QUOTE_CONFIG.MOVEMENT_RANGE_X}px, ${QUOTE_CONFIG.MOVEMENT_RANGE_Y}px);
    }
    40% {
      transform: translate(${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.5}px, -${QUOTE_CONFIG.MOVEMENT_RANGE_Y * 0.7}px);
    }
    60% {
      transform: translate(-${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.8}px, ${QUOTE_CONFIG.MOVEMENT_RANGE_Y * 0.3}px);
    }
    80% {
      transform: translate(-${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.2}px, -${QUOTE_CONFIG.MOVEMENT_RANGE_Y}px);
    }
  }
  
  .floating-quote {
    --opacity: 1;
    --rotation: ${QUOTE_CONFIG.ROTATION};
    animation-name: float-up;
    animation-timing-function: linear;
    animation-iteration-count: infinite;
    will-change: transform, opacity;
    transform: translateX(-50%) rotate(var(--rotation));
    z-index: 0; /* Ensure quotes are behind the UI */
  }
  
  .quote-star {
    color: ${QUOTE_CONFIG.DOT_COLOR};
    font-size: ${QUOTE_CONFIG.DOT_SIZE};
    display: inline-block;
    margin-left: 0.1em;
    animation: star-pulse ${QUOTE_CONFIG.DOT_PULSE_DURATION} ease-in-out infinite${QUOTE_CONFIG.MOVEMENT_ENABLED ? `, float-around ${QUOTE_CONFIG.MOVEMENT_DURATION}s ease-in-out infinite` : ''};
    will-change: opacity, transform, text-shadow;
    position: relative;
  }
`;

// Helper component for rendering a floating quote
export const renderFloatingQuote = (praise: string, style: QuoteStyle, idx: number) => {
  return (
    <div
      key={idx}
      className="absolute whitespace-nowrap select-none floating-quote"
      style={{ 
        left: style.left,
        top: style.top,
        fontSize: style.fontSize,
        opacity: style.opacity,
        animationDuration: style.animationDuration,
        animationDelay: style.animationDelay,
        color: style.color,
        letterSpacing: style.letterSpacing,
        '--rotation': style.rotation,
      } as React.CSSProperties}
    >
      {praise}
      {renderPulsingStar()}
    </div>
  );
};

// Helper component for rendering a pulsing star/dot
// This can be used in any component to add a pulsing star/dot
export const renderPulsingStar = (customProps?: {
  char?: string;
  color?: string;
  size?: string;
  className?: string;
  disableMovement?: boolean;
}) => {
  const char = customProps?.char || QUOTE_CONFIG.DOT_CHAR;
  // Always include quote-star class to ensure animation is applied
  const className = customProps?.className 
    ? `quote-star ${customProps.className}`
    : 'quote-star';
  
  // Determine if movement should be enabled
  const enableMovement = QUOTE_CONFIG.MOVEMENT_ENABLED && !customProps?.disableMovement;
  
  // Add randomness to animation timing if random movement is enabled
  const randomDelay = QUOTE_CONFIG.MOVEMENT_RANDOM 
    ? `${-(Math.random() * QUOTE_CONFIG.MOVEMENT_DURATION)}s` 
    : '0s';
  
  // Create animation string with both pulse and movement (if enabled)
  const animationString = enableMovement
    ? `star-pulse ${QUOTE_CONFIG.DOT_PULSE_DURATION}s ease-in-out infinite, float-around ${QUOTE_CONFIG.MOVEMENT_DURATION}s ease-in-out infinite`
    : `star-pulse ${QUOTE_CONFIG.DOT_PULSE_DURATION}s ease-in-out infinite`;
  
  return (
    <span 
      className={className}
      style={{
        color: customProps?.color || QUOTE_CONFIG.DOT_COLOR,
        fontSize: customProps?.size || QUOTE_CONFIG.DOT_SIZE,
        animationDelay: `${-(Math.random() * 3)}s, ${randomDelay}`, // Random delays for pulse and movement
        display: 'inline-block',
        animation: animationString,
        willChange: 'opacity, transform, text-shadow',
        position: 'relative',
      }}
    >
      {char}
    </span>
  );
};
