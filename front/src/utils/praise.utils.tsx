// Configuration constants for the floating quotes
export const QUOTE_CONFIG = {
  // Quote selection
  MIN_QUOTES: 8, // Minimum number of quotes to display (reduced to prevent overcrowding)
  MAX_QUOTES: 12, // Maximum number of quotes to display (reduced to prevent overcrowding)

  // Positioning
  LEFT_SIDE_MIN: 15, // Minimum position for left side quotes (%)
  LEFT_SIDE_MAX: 35, // Maximum position for left side quotes (%)
  RIGHT_SIDE_MIN: 65, // Minimum position for right side quotes (%)
  RIGHT_SIDE_MAX: 85, // Maximum position for right side quotes (%)
  TOP_MIN: 5, // Minimum vertical position (%)
  TOP_MAX: 95, // Maximum vertical position (%)
  VERTICAL_SEGMENTS: 10, // Number of vertical segments for better distribution (increased)

  // Movement
  MOVEMENT_ENABLED: true, // Enable multi-directional movement for dots
  MOVEMENT_RANGE_X: 1.5, // Horizontal movement range (px)
  MOVEMENT_RANGE_Y: 1.5, // Vertical movement range (px)
  MOVEMENT_DURATION: 5, // Movement animation duration (seconds)
  MOVEMENT_RANDOM: true, // Use random movement pattern

  // Sizing
  MIN_FONT_SIZE: 0.8, // Minimum font size (rem)
  MAX_FONT_SIZE: 1.8, // Maximum font size (rem)

  // Appearance
  MIN_OPACITY: 0.5, // Minimum opacity
  MAX_OPACITY: 0.75, // Maximum opacity
  TEXT_COLOR: "rgba(100,116,139,0.6)", // Text color

  // dot configuration
  DOT_COLOR: "rgba(255, 191, 0, 1)", // Base color for the dot
  DOT_GLOW_COLOR: "rgba(255, 215, 0, 0.6)", // Glow color for the dot
  DOT_SIZE: "1.4em", // Size of the dot relative to text
  DOT_PULSE_DURATION: "3s", // Duration of the pulse animation
  DOT_PULSE_MIN: 0.3, // Minimum opacity during pulse
  DOT_PULSE_MAX: 1, // Maximum opacity during pulse
  DOT_CHAR: ".", // Character to use for the dot

  LETTER_SPACING: "-0.02em", // Letter spacing
  ROTATION: "0deg", // Rotation (set to 0 for upright quotes)

  // Animation
  MIN_DURATION: 90, // Minimum animation duration (seconds)
  MAX_DURATION: 90, // Maximum animation duration (seconds)
  MAX_DELAY: 60, // Maximum animation delay (seconds)

  // Overlap prevention
  WIDTH_FACTOR: 0.5, // Factor to estimate width based on text length and font size
  MIN_HORIZONTAL_SPACING: 1000.0, // Minimum horizontal spacing between quotes (multiplier of combined widths)
  MIN_VERTICAL_SPACING: 1000.0, // Minimum vertical spacing between quotes (multiplier of combined heights)
  MAX_PLACEMENT_ATTEMPTS: 50, // Maximum attempts to find non-overlapping position (increased)
};

// All praises - please add more
// I think it would be fun if these reflected our life during developement - something to remember in the future
export const ALL_PRAISES = [
  "Eigintas: einu ginti",
  "the idea of komcon. was born on a couch on a wonderful autumn evening",
  "Iš komandos ‘kom’, iš connect ‘con’, o taškas – nes viskas tuo ir pasakyta. komcon",
  "Nojus meta veipint (nuo naujų metų), bet niekada nemes komcon'o",
  "Ugnius iš Biržų – bringing northern Lithuanian energy since forever",
  "Nojus iš Alytaus – certified heatwave playlist creator",
  "Uniting friends faster than your Wi-Fi finds ‘Kaimynas_5G’",
  "Where melodies meet komanda spirit",
  "Ugnius gavo 69 iš lietuvių ir fizikos VBE",
  "Alytiškis + Vilnietis + Biržietis + Sofa = ultimate startup formula",
  "Good music. Good friends. Good vibes. Zero veipas (nuo naujųjų)",
  "Our playlists slap harder than Sel'as Alytuj",
  "We bring people together. Music just helps",
  "Kasijus: would pilot a planet, but he's too busy leading komcon",
  "Making collaborative playlists cooler than they have any right to be",
  "Peace, friendship, and mildly questionable song choices",
  "Your friends choose songs, you choose violence (kidding… or?)",
  "North meets South. Beats meet hearts. Biržai meets Alytus",
  "Net Spotify'us komcon. backend'ą svajoja pasisavinti",
  "komcon.: the most iconic sofa-born idea since pillows",
  "Ugnius is the best chef - he cooked komcon. like Gordon Ramsay cooks beef",
  "komcon. appreciates everyone... Especially Klaudijus",
  "Alytus and Birzai may be far from each other, but komcon. is where they meet",
  "It's not about the time we spent on komcon., it's about the friends we made along the way",
  "People come and go, but if one can't come no one can",
  "Kasijus can't crash komcon. but a bolt car... It's a different story",
  "While other teams play checkers komcon. plays chess",
  "komcon. doesn't have any ties with IBM...",
  "komcon. always plans ahead, procrastination is not in the vocabulary",
  "We have no enemies apart from that one HDMI adapter",
  "komcon. > funkcinio paskaitos",
  "16:00?",
  '"Fixed bug" - 5000 lines changed',
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
  const count =
    Math.floor(
      Math.random() * (QUOTE_CONFIG.MAX_QUOTES - QUOTE_CONFIG.MIN_QUOTES + 1)
    ) + QUOTE_CONFIG.MIN_QUOTES;
  return shuffled.slice(0, count);
};

// Generate random positions and sizes like scattered leaves
export const generatePraiseStyles = (praises: string[]): QuoteStyle[] => {
  // Distribute quotes across the entire width
  const positions: QuotePosition[] = [];

  // Pre-assign vertical segments to ensure even distribution
  const leftSegments = Array(QUOTE_CONFIG.VERTICAL_SEGMENTS)
    .fill(0)
    .map((_, i) => i);
  const rightSegments = Array(QUOTE_CONFIG.VERTICAL_SEGMENTS)
    .fill(0)
    .map((_, i) => i);

  // Shuffle the segments
  leftSegments.sort(() => 0.5 - Math.random());
  rightSegments.sort(() => 0.5 - Math.random());

  // Distribute quotes evenly between left and right sides
  const leftCount = Math.ceil(praises.length / 2);

  // Create a more natural distribution of quotes
  return praises.map((praise, index) => {
    // Determine if this quote should be on the left or right side
    const isLeftSide = index < leftCount;

    // Get the next available segment for this side
    const segmentIndex = isLeftSide
      ? leftSegments[index % leftSegments.length]
      : rightSegments[(index - leftCount) % rightSegments.length];
    // Generate random position across the entire width
    let leftPosition: number = 0;
    let topPosition: number = 0;
    let fontSize: number = 0;
    let attempts = 0;

    // Keep trying positions until we find one that doesn't overlap
    do {
      // Position quotes on the predetermined side
      if (isLeftSide) {
        leftPosition =
          Math.random() *
            (QUOTE_CONFIG.LEFT_SIDE_MAX - QUOTE_CONFIG.LEFT_SIDE_MIN) +
          QUOTE_CONFIG.LEFT_SIDE_MIN;
      } else {
        leftPosition =
          Math.random() *
            (QUOTE_CONFIG.RIGHT_SIDE_MAX - QUOTE_CONFIG.RIGHT_SIDE_MIN) +
          QUOTE_CONFIG.RIGHT_SIDE_MIN;
      }

      // Use the pre-assigned vertical segment for better distribution
      const segmentHeight =
        (QUOTE_CONFIG.TOP_MAX - QUOTE_CONFIG.TOP_MIN) /
        QUOTE_CONFIG.VERTICAL_SEGMENTS;

      // Calculate position within the segment (with some randomness, but limited to prevent overlap)
      const segmentTop = QUOTE_CONFIG.TOP_MIN + segmentIndex * segmentHeight;
      topPosition =
        segmentTop +
        (Math.random() * (segmentHeight * 0.8) + segmentHeight * 0.1);

      // Smaller font sizes to ensure quotes stay within screen
      fontSize =
        Math.random() *
          (QUOTE_CONFIG.MAX_FONT_SIZE - QUOTE_CONFIG.MIN_FONT_SIZE) +
        QUOTE_CONFIG.MIN_FONT_SIZE;

      attempts++;
    } while (
      attempts < QUOTE_CONFIG.MAX_PLACEMENT_ATTEMPTS &&
      positions.some((pos) => {
        // Calculate distance between quotes
        const yDist = Math.abs(pos.top - topPosition);

        // Since we're using pre-assigned segments, quotes on different sides can't overlap
        // and quotes in different segments shouldn't overlap vertically
        // Just check for quotes in the same segment or adjacent segments

        // Check if quotes are on the same side
        const onSameSide =
          (leftPosition < 50 && pos.left < 50) || // Both on left side
          (leftPosition >= 50 && pos.left >= 50); // Both on right side

        if (!onSameSide) {
          // Quotes on different sides don't overlap
          return false;
        }

        // For quotes on the same side, check if they're too close vertically
        // Use a percentage-based approach instead of font-size based
        const minVerticalDistance = 10; // Minimum 10% vertical distance
        return yDist < minVerticalDistance;
      })
    );

    // Increase opacity to make quotes more visible
    const opacity =
      Math.random() * (QUOTE_CONFIG.MAX_OPACITY - QUOTE_CONFIG.MIN_OPACITY) +
      QUOTE_CONFIG.MIN_OPACITY;

    // Vary animation duration for more natural movement
    const animationDuration =
      Math.random() * (QUOTE_CONFIG.MAX_DURATION - QUOTE_CONFIG.MIN_DURATION) +
      QUOTE_CONFIG.MIN_DURATION;

    // Store this position to check for future overlaps
    const position = {
      left: leftPosition,
      top: topPosition,
      fontSize,
      opacity,
      animationDuration,
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
      text-shadow: 0 0 15px ${QUOTE_CONFIG.DOT_GLOW_COLOR}, 0 0 20px ${
  QUOTE_CONFIG.DOT_GLOW_COLOR
};
    }
  }
  
  @keyframes float-around {
    0%, 100% {
      transform: translate(0, 0);
    }
    20% {
      transform: translate(${QUOTE_CONFIG.MOVEMENT_RANGE_X}px, ${
  QUOTE_CONFIG.MOVEMENT_RANGE_Y
}px);
    }
    40% {
      transform: translate(${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.5}px, -${
  QUOTE_CONFIG.MOVEMENT_RANGE_Y * 0.7
}px);
    }
    60% {
      transform: translate(-${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.8}px, ${
  QUOTE_CONFIG.MOVEMENT_RANGE_Y * 0.3
}px);
    }
    80% {
      transform: translate(-${QUOTE_CONFIG.MOVEMENT_RANGE_X * 0.2}px, -${
  QUOTE_CONFIG.MOVEMENT_RANGE_Y
}px);
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
    animation: star-pulse ${
      QUOTE_CONFIG.DOT_PULSE_DURATION
    } ease-in-out infinite${
  QUOTE_CONFIG.MOVEMENT_ENABLED
    ? `, float-around ${QUOTE_CONFIG.MOVEMENT_DURATION}s ease-in-out infinite`
    : ""
};
    will-change: opacity, transform, text-shadow;
    position: relative;
  }
`;

// Helper component for rendering a floating quote
export const renderFloatingQuote = (
  praise: string,
  style: QuoteStyle,
  idx: number
) => {
  return (
    <div
      key={idx}
      className="absolute whitespace-nowrap select-none floating-quote"
      style={
        {
          left: style.left,
          top: style.top,
          fontSize: style.fontSize,
          opacity: style.opacity,
          animationDuration: style.animationDuration,
          animationDelay: style.animationDelay,
          color: style.color,
          letterSpacing: style.letterSpacing,
          "--rotation": style.rotation,
        } as React.CSSProperties
      }
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
    : "quote-star";

  // Determine if movement should be enabled
  const enableMovement =
    QUOTE_CONFIG.MOVEMENT_ENABLED && !customProps?.disableMovement;

  // Add randomness to animation timing if random movement is enabled
  const randomDelay = QUOTE_CONFIG.MOVEMENT_RANDOM
    ? `${-(Math.random() * QUOTE_CONFIG.MOVEMENT_DURATION)}s`
    : "0s";

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
        display: "inline-block",
        animation: animationString,
        willChange: "opacity, transform, text-shadow",
        position: "relative",
      }}
    >
      {char}
    </span>
  );
};
